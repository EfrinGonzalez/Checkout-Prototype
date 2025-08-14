namespace Payments.Domain;

public enum PaymentStatus { Initiated, Authorized, Failed, Captured }

public class PaymentIntent
{
    public string PaymentId { get; private set; } = null!;
    public Guid OrderId { get; private set; }
    public string Currency { get; private set; } = "DKK";
    public int AmountMinor { get; private set; }
    public string HostedPaymentPageUrl { get; private set; } = null!;
    public PaymentStatus Status { get; private set; } = PaymentStatus.Initiated;
    public DateTime CreatedAtUtc { get; private set; } = DateTime.UtcNow;
    public PaymentIntent(string paymentId, Guid orderId, string currency, int amountMinor, string hostedUrl)
    { PaymentId = paymentId; OrderId = orderId; Currency = currency; AmountMinor = amountMinor; HostedPaymentPageUrl = hostedUrl; }
    public void MarkAuthorized() => Status = PaymentStatus.Authorized;
    public void MarkFailed() => Status = PaymentStatus.Failed;
    public void MarkCaptured() => Status = PaymentStatus.Captured;
}
