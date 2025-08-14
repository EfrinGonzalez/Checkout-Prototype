using Microsoft.AspNetCore.Mvc;
using MediatR;
using Payments.Application;
using Shared.Contracts;
using System.Text.Json;

namespace Payments.WebApi.Controllers;

[ApiController]
[Route("api/webhooks/nexi")]
public class WebhooksController : ControllerBase
{
    private readonly IMediator _mediator; private readonly IHttpClientFactory _httpFactory;
    public WebhooksController(IMediator mediator, IHttpClientFactory httpFactory){ _mediator = mediator; _httpFactory = httpFactory; }

    [HttpPost]
    public async Task<IActionResult> Handle([FromBody] JsonElement payload, CancellationToken ct)
    {
        var paymentId = payload.GetProperty("paymentId").GetString();
        var eventType = payload.GetProperty("event").GetString();
        var orderIdStr = payload.TryGetProperty("orderId", out var oid) ? oid.GetString() : null;
        if (paymentId is null || eventType is null || orderIdStr is null) return BadRequest();

        if (eventType.Contains("completed"))
            await _mediator.Send(new Commands.MarkAuthorized(paymentId), ct);
        else if (eventType.Contains("failed"))
            await _mediator.Send(new Commands.MarkFailed(paymentId, "failed"), ct);

        var client = _httpFactory.CreateClient("Orders");
        var status = eventType.Contains("completed") ? "Authorized" : "Failed";
        await client.PutAsJsonAsync($"api/orders/{orderIdStr}/status", new UpdateOrderStatusRequest(status), ct);
        return Ok();
    }

    [HttpPost("simulate/authorized/{paymentId}/{orderId}")]
    public async Task<IActionResult> SimAuthorized([FromRoute] string paymentId, [FromRoute] Guid orderId, CancellationToken ct)
    {
        await _mediator.Send(new Commands.MarkAuthorized(paymentId), ct);
        var client = _httpFactory.CreateClient("Orders");
        await client.PutAsJsonAsync($"api/orders/{orderId}/status", new UpdateOrderStatusRequest("Authorized"), ct);
        return Ok();
    }
}
