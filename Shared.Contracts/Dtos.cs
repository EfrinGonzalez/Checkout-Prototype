namespace Shared.Contracts;

// --- Shop → Orders ---
public sealed record CreateOrderRequest(
    string UserId,
    string Currency,
    decimal SubtotalNet,
    decimal Tax,
    decimal Shipping,
    decimal TotalGross,
    IReadOnlyList<OrderLineDto> Lines
);

public sealed record CreateOrderResponse(string OrderId);

public sealed record OrderLineDto(
    string Sku,
    string Name,
    int Quantity,
    decimal UnitPrice,
    int TaxRateBasisPoints,
    decimal Net,
    decimal Tax,
    decimal Gross
);

// --- Payments → Orders ---
public sealed record UpdateOrderStatusRequest(string Status, string? Reason = null);

// --- Shop → Payments ---
public sealed record InitiatePaymentRequest(
    string OrderId,
    string Currency,
    int AmountMinor,
    string ReturnUrl,
    string CancelUrl
);

public sealed record InitiatePaymentResponse(string PaymentId, string HostedPaymentPageUrl);

// --- Read models ---
public sealed record OrderReadModel(
    string OrderId,
    string UserId,
    string Currency,
    decimal SubtotalNet,
    decimal Tax,
    decimal Shipping,
    decimal TotalGross,
    string? PaymentId,
    string Status
);
