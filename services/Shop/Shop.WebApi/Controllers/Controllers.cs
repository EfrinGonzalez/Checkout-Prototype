using Microsoft.AspNetCore.Mvc;
using MediatR;
using Shop.Application;

namespace Shop.WebApi.Controllers;

[ApiController]
[Route("api/catalog")]
public class CatalogController : ControllerBase
{
    private readonly IMediator _mediator;
    public CatalogController(IMediator mediator) => _mediator = mediator;

    [HttpGet("products")]
    public Task<IReadOnlyList<ProductDto>> Get(CancellationToken ct) => _mediator.Send(new Queries.GetCatalog(), ct);
}

[ApiController]
[Route("api/basket")]
public class BasketController : ControllerBase
{
    private readonly IMediator _mediator;
    public BasketController(IMediator mediator) => _mediator = mediator;

    [HttpGet]
    public Task<BasketDto> Get([FromQuery] string userId = "demo-user", CancellationToken ct = default)
        => _mediator.Send(new Queries.GetBasket(userId), ct);

    public sealed record AddItemDto(Guid ProductId, int Quantity);

    [HttpPost("items")]
    public async Task<IActionResult> Add([FromBody] AddItemDto dto, [FromQuery] string userId = "demo-user", CancellationToken ct = default)
    {
        await _mediator.Send(new Commands.AddItem(userId, dto.ProductId, dto.Quantity), ct);
        return NoContent();
    }

    [HttpDelete("items/{productId:guid}")]
    public async Task<IActionResult> Remove([FromRoute] Guid productId, [FromQuery] string userId = "demo-user", CancellationToken ct = default)
    {
        await _mediator.Send(new Commands.RemoveItem(userId, productId), ct);
        return NoContent();
    }

    [HttpDelete]
    public async Task<IActionResult> Clear([FromQuery] string userId = "demo-user", CancellationToken ct = default)
    {
        await _mediator.Send(new Commands.ClearBasket(userId), ct);
        return NoContent();
    }
}

[ApiController]
[Route("api/checkout")]
public class CheckoutController : ControllerBase
{
    private readonly IMediator _mediator;
    public CheckoutController(IMediator mediator) => _mediator = mediator;

    public sealed record StartDto(string ReturnUrl, string CancelUrl);

    [HttpPost("start")]
    public Task<StartCheckoutResponse> Start([FromBody] StartDto dto, [FromQuery] string userId = "demo-user", CancellationToken ct = default)
        => _mediator.Send(new Commands.StartCheckout(userId, dto.ReturnUrl, dto.CancelUrl), ct);
}
