using MediatR;
using Shared.Contracts;
using Orders.Domain;

namespace Orders.Application;

public static class Commands
{
    public sealed record CreateOrder(CreateOrderRequest Request) : IRequest<CreateOrderResponse>;
    public sealed class CreateOrderHandler : IRequestHandler<CreateOrder, CreateOrderResponse>
    {
        private readonly IOrdersRepository _repo;
        public CreateOrderHandler(IOrdersRepository repo) => _repo = repo;
        public async Task<CreateOrderResponse> Handle(CreateOrder request, CancellationToken ct)
        {
            var r = request.Request;
            var order = new Order(r.UserId, r.Currency, r.SubtotalNet, r.Tax, r.Shipping, r.TotalGross);
            foreach (var l in r.Lines)
                order.Lines.Add(new OrderLine(order.Id, l.Sku, l.Name, l.Quantity, l.UnitPrice, l.TaxRateBasisPoints, l.Net, l.Tax, l.Gross));
            await _repo.AddAsync(order, ct);
            return new CreateOrderResponse(order.Id.ToString());
        }
    }

    public sealed record UpdateStatus(Guid OrderId, UpdateOrderStatusRequest Request) : IRequest<Unit>;
    public sealed class UpdateStatusHandler : IRequestHandler<UpdateStatus, Unit>
    {
        private readonly IOrdersRepository _repo;
        public UpdateStatusHandler(IOrdersRepository repo) => _repo = repo;
        public async Task<Unit> Handle(UpdateStatus cmd, CancellationToken ct)
        {
            var order = await _repo.GetAsync(cmd.OrderId, ct) ?? throw new InvalidOperationException("Order not found");
            if (!Enum.TryParse<OrderStatus>(cmd.Request.Status, out var newStatus))
                throw new InvalidOperationException("Invalid status");
            order.SetStatus(newStatus);
            await _repo.SaveChangesAsync(ct);
            return Unit.Value;
        }
    }
}

public static class Queries
{
    public sealed record GetOrder(Guid OrderId) : IRequest<OrderReadModel>;
    public sealed class GetOrderHandler : IRequestHandler<GetOrder, OrderReadModel>
    {
        private readonly IOrdersRepository _repo;
        public GetOrderHandler(IOrdersRepository repo) => _repo = repo;
        public async Task<OrderReadModel> Handle(GetOrder q, CancellationToken ct)
        {
            var o = await _repo.GetAsync(q.OrderId, ct) ?? throw new InvalidOperationException("Order not found");
            return new OrderReadModel(o.Id.ToString(), o.UserId, o.Currency, o.SubtotalNet, o.Tax, o.Shipping, o.TotalGross, o.PaymentId, o.Status.ToString());
        }
    }
}

public interface IOrdersRepository
{
    Task AddAsync(Order order, CancellationToken ct);
    Task<Order?> GetAsync(Guid id, CancellationToken ct);
    Task SaveChangesAsync(CancellationToken ct);
}
