using Microsoft.AspNetCore.Mvc;
using MediatR;
using Orders.Application;
using Shared.Contracts;

namespace Orders.WebApi.Controllers;

[ApiController]
[Route("api/orders")]
public class OrdersController : ControllerBase
{
    private readonly IMediator _mediator;
    public OrdersController(IMediator mediator) => _mediator = mediator;

    [HttpPost]
    public async Task<ActionResult<CreateOrderResponse>> Create([FromBody] CreateOrderRequest req, CancellationToken ct)
    {
        var id = await _mediator.Send(new Commands.CreateOrder(req), ct);
        return CreatedAtAction(nameof(Get), new { orderId = id.OrderId }, id);
    }

    [HttpGet("{orderId:guid}")]
    public async Task<ActionResult<OrderReadModel>> Get([FromRoute] Guid orderId, CancellationToken ct)
    {
        var order = await _mediator.Send(new Queries.GetOrder(orderId), ct);
        return Ok(order);
    }

    [HttpPut("{orderId:guid}/status")]
    public async Task<IActionResult> UpdateStatus([FromRoute] Guid orderId, [FromBody] UpdateOrderStatusRequest req, CancellationToken ct)
    {
        await _mediator.Send(new Commands.UpdateStatus(orderId, req), ct);
        return NoContent();
    }
}
