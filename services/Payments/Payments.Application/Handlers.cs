using MediatR;
using Shared.Contracts;
using Payments.Domain;

namespace Payments.Application;

public static class Commands
{
    public sealed record InitiatePayment(InitiatePaymentRequest Request) : IRequest<InitiatePaymentResponse>;
    public sealed record MarkAuthorized(string PaymentId) : IRequest;
    public sealed record MarkFailed(string PaymentId, string? Reason) : IRequest;
    public sealed record MarkCaptured(string PaymentId) : IRequest;
}

public interface IPaymentsRepository
{
    Task AddAsync(PaymentIntent intent, CancellationToken ct);
    Task<PaymentIntent?> GetAsync(string paymentId, CancellationToken ct);
    Task SaveChangesAsync(CancellationToken ct);
}

public interface INexiClient
{
    Task<(string paymentId, string hostedUrl)> CreatePaymentAsync(string orderId, string currency, int amountMinor, string returnUrl, string cancelUrl, CancellationToken ct);
    Task CaptureAsync(string paymentId, int amountMinor, CancellationToken ct);
}

public sealed class InitiatePaymentHandler : IRequestHandler<Commands.InitiatePayment, InitiatePaymentResponse>
{
    private readonly IPaymentsRepository _repo; private readonly INexiClient _nexi;
    public InitiatePaymentHandler(IPaymentsRepository repo, INexiClient nexi) { _repo = repo; _nexi = nexi; }
    public async Task<InitiatePaymentResponse> Handle(Commands.InitiatePayment cmd, CancellationToken ct)
    {
        var r = cmd.Request;
        var (pid, url) = await _nexi.CreatePaymentAsync(r.OrderId, r.Currency, r.AmountMinor, r.ReturnUrl, r.CancelUrl, ct);
        var intent = new PaymentIntent(pid, Guid.Parse(r.OrderId), r.Currency, r.AmountMinor, url);
        await _repo.AddAsync(intent, ct);
        return new InitiatePaymentResponse(pid, url);
    }
}

public sealed class MarkAuthorizedHandler : IRequestHandler<Commands.MarkAuthorized, Unit>
{ private readonly IPaymentsRepository _repo; public MarkAuthorizedHandler(IPaymentsRepository repo)=>_repo=repo; public async Task<Unit> Handle(Commands.MarkAuthorized cmd, CancellationToken ct){ var p=await _repo.GetAsync(cmd.PaymentId, ct)??throw new InvalidOperationException("Payment not found"); p.MarkAuthorized(); await _repo.SaveChangesAsync(ct); return Unit.Value; } }
public sealed class MarkFailedHandler : IRequestHandler<Commands.MarkFailed, Unit>
{ private readonly IPaymentsRepository _repo; public MarkFailedHandler(IPaymentsRepository repo)=>_repo=repo; public async Task<Unit> Handle(Commands.MarkFailed cmd, CancellationToken ct){ var p=await _repo.GetAsync(cmd.PaymentId, ct)??throw new InvalidOperationException("Payment not found"); p.MarkFailed(); await _repo.SaveChangesAsync(ct); return Unit.Value; } }
public sealed class MarkCapturedHandler : IRequestHandler<Commands.MarkCaptured, Unit>
{ private readonly IPaymentsRepository _repo; private readonly INexiClient _nexi; public MarkCapturedHandler(IPaymentsRepository repo, INexiClient nexi){_repo=repo;_nexi=nexi;} public async Task<Unit> Handle(Commands.MarkCaptured cmd, CancellationToken ct){ var p=await _repo.GetAsync(cmd.PaymentId, ct)??throw new InvalidOperationException("Payment not found"); await _nexi.CaptureAsync(cmd.PaymentId, p.AmountMinor, ct); p.MarkCaptured(); await _repo.SaveChangesAsync(ct); return Unit.Value; } }
