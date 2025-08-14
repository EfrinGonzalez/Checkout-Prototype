using MediatR;
using Shared.Contracts;
using Shop.Domain;

namespace Shop.Application;

public static class Commands
{
    public sealed record AddItem(string UserId, Guid ProductId, int Quantity) : IRequest;
    public sealed record RemoveItem(string UserId, Guid ProductId) : IRequest;
    public sealed record ClearBasket(string UserId) : IRequest;
    public sealed record StartCheckout(string UserId, string ReturnUrl, string CancelUrl) : IRequest<StartCheckoutResponse>;
}
public static class Queries
{
    public sealed record GetBasket(string UserId) : IRequest<BasketDto>;
    public sealed record GetCatalog() : IRequest<IReadOnlyList<ProductDto>>;
}
public sealed record BasketDto(string UserId, IReadOnlyList<BasketLineDto> Lines, decimal Subtotal, int TaxRateBps, decimal Tax, decimal Total);
public sealed record BasketLineDto(Guid ProductId, string Sku, string Name, int Quantity, decimal UnitPrice, decimal LineGross);
public sealed record ProductDto(Guid ProductId, string Sku, string Name, decimal Price);
public sealed record StartCheckoutResponse(string OrderId, string PaymentId, string HostedPaymentPageUrl);
public interface IShopRepository { Task<Basket> GetOrCreateBasketAsync(string userId, CancellationToken ct); Task SaveChangesAsync(CancellationToken ct); IQueryable<Product> Products { get; } }
public interface IOrdersClient { Task<string> CreateOrderAsync(CreateOrderRequest request, CancellationToken ct); }
public interface IPaymentsClient { Task<InitiatePaymentResponse> InitiateAsync(InitiatePaymentRequest request, CancellationToken ct); }
public sealed class AddItemHandler : IRequestHandler<Commands.AddItem, Unit>
{ private readonly IShopRepository _repo; public AddItemHandler(IShopRepository repo)=>_repo=repo; public async Task<Unit> Handle(Commands.AddItem cmd, CancellationToken ct){ var basket=await _repo.GetOrCreateBasketAsync(cmd.UserId, ct); var product=_repo.Products.First(p=>p.Id==cmd.ProductId); basket.AddItem(product.Id, product.Sku, product.Name, product.Price, cmd.Quantity, 2500); await _repo.SaveChangesAsync(ct); return Unit.Value; } }
public sealed class RemoveItemHandler : IRequestHandler<Commands.RemoveItem, Unit>
{ private readonly IShopRepository _repo; public RemoveItemHandler(IShopRepository repo)=>_repo=repo; public async Task<Unit> Handle(Commands.RemoveItem cmd, CancellationToken ct){ var basket=await _repo.GetOrCreateBasketAsync(cmd.UserId, ct); basket.RemoveItem(cmd.ProductId); await _repo.SaveChangesAsync(ct); return Unit.Value; } }
public sealed class ClearBasketHandler : IRequestHandler<Commands.ClearBasket, Unit>
{ private readonly IShopRepository _repo; public ClearBasketHandler(IShopRepository repo)=>_repo=repo; public async Task<Unit> Handle(Commands.ClearBasket cmd, CancellationToken ct){ var basket=await _repo.GetOrCreateBasketAsync(cmd.UserId, ct); basket.Clear(); await _repo.SaveChangesAsync(ct); return Unit.Value; } }
public sealed class GetBasketHandler : IRequestHandler<Queries.GetBasket, BasketDto>
{
    private readonly IShopRepository _repo; public GetBasketHandler(IShopRepository repo)=>_repo=repo;
    public async Task<BasketDto> Handle(Queries.GetBasket q, CancellationToken ct)
    { var b=await _repo.GetOrCreateBasketAsync(q.UserId, ct); var lines=b.Items.Select(i=> new BasketLineDto(i.ProductId, i.Sku, i.Name, i.Quantity, i.UnitPrice, i.UnitPrice*i.Quantity*1.25m)).ToList(); var subtotal=b.Items.Sum(i=>i.UnitPrice*i.Quantity); var tax=Math.Round(subtotal*0.25m,2); var total=subtotal+tax; return new BasketDto(b.UserId, lines, subtotal, 2500, tax, total); }
}
public sealed class GetCatalogHandler : IRequestHandler<Queries.GetCatalog, IReadOnlyList<ProductDto>>
{ private readonly IShopRepository _repo; public GetCatalogHandler(IShopRepository repo)=>_repo=repo; public Task<IReadOnlyList<ProductDto>> Handle(Queries.GetCatalog q, CancellationToken ct){ var list=_repo.Products.Select(p=>new ProductDto(p.Id,p.Sku,p.Name,p.Price)).ToList(); return Task.FromResult<IReadOnlyList<ProductDto>>(list);} }
public sealed class StartCheckoutHandler : IRequestHandler<Commands.StartCheckout, StartCheckoutResponse>
{
    private readonly IShopRepository _repo; private readonly IOrdersClient _orders; private readonly IPaymentsClient _payments;
    public StartCheckoutHandler(IShopRepository repo, IOrdersClient orders, IPaymentsClient payments){ _repo=repo; _orders=orders; _payments=payments; }
    public async Task<StartCheckoutResponse> Handle(Commands.StartCheckout cmd, CancellationToken ct)
    {
        var basket = await _repo.GetOrCreateBasketAsync(cmd.UserId, ct);
        var subtotal = basket.Items.Sum(i => i.UnitPrice * i.Quantity);
        var tax = Math.Round(subtotal * 0.25m, 2);
        var total = subtotal + tax;
        var lines = basket.Items.Select(i => new OrderLineDto(i.Sku, i.Name, i.Quantity, i.UnitPrice, i.TaxRateBasisPoints, i.UnitPrice*i.Quantity, Math.Round(i.UnitPrice*i.Quantity*0.25m,2), Math.Round(i.UnitPrice*i.Quantity*1.25m,2))).ToList();
        var createReq = new CreateOrderRequest(cmd.UserId, "DKK", subtotal, tax, 0m, total, lines);
        var orderId = await _orders.CreateOrderAsync(createReq, ct);
        int toMinor(decimal d) => (int)Math.Round(d * 100m);
        var payReq = new InitiatePaymentRequest(orderId, "DKK", toMinor(total), cmd.ReturnUrl, cmd.CancelUrl);
        var payRes = await _payments.InitiateAsync(payReq, ct);
        return new StartCheckoutResponse(orderId, payRes.PaymentId, payRes.HostedPaymentPageUrl);
    }
}
