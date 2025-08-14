using Microsoft.AspNetCore.Mvc;
using MediatR;
using Payments.Application;
using Shared.Contracts;

namespace Payments.WebApi.Controllers;

[ApiController]
[Route("api/payments")]
public class PaymentsController : ControllerBase
{
    private readonly IMediator _mediator;
    public PaymentsController(IMediator mediator) => _mediator = mediator;

    [HttpPost("initiate")]
    public async Task<ActionResult<InitiatePaymentResponse>> Initiate([FromBody] InitiatePaymentRequest req, CancellationToken ct)
        => Ok(await _mediator.Send(new Commands.InitiatePayment(req), ct));

    [HttpPost("{paymentId}/capture")]
    public async Task<IActionResult> Capture([FromRoute] string paymentId, CancellationToken ct)
    { await _mediator.Send(new Commands.MarkCaptured(paymentId), ct); return NoContent(); }
}
