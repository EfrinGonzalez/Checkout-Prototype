namespace Orders.Domain;

public enum OrderStatus { Pending, Authorized, Failed, Captured, Cancelled }

public class Order
{
    public Guid Id { get; private set; } = Guid.NewGuid();
    public string UserId { get; private set; } = null!;
    public string Currency { get; private set; } = "DKK";
    public decimal SubtotalNet { get; private set; }
    public decimal Tax { get; private set; }
    public decimal Shipping { get; private set; }
    public decimal TotalGross { get; private set; }
    public string? PaymentId { get; private set; }
    public OrderStatus Status { get; private set; } = OrderStatus.Pending;
    public DateTime CreatedAtUtc { get; private set; } = DateTime.UtcNow;
    public List<OrderLine> Lines { get; private set; } = new();
    private Order() { } // ← EF uses this
    public Order(string userId, string currency, decimal subtotal, decimal tax, decimal shipping, decimal total)
    { UserId = userId; Currency = currency; SubtotalNet = subtotal; Tax = tax; Shipping = shipping; TotalGross = total; Status = OrderStatus.Pending; }
    public void SetPaymentId(string paymentId) => PaymentId = paymentId;
    public void SetStatus(OrderStatus status) => Status = status;
}
public class OrderLine
{
    public Guid Id { get; private set; } = Guid.NewGuid();
    public Guid OrderId { get; private set; }
    public string Sku { get; private set; } = null!;
    public string Name { get; private set; } = null!;
    public int Quantity { get; private set; }
    public decimal UnitPrice { get; private set; }
    public int TaxRateBasisPoints { get; private set; }
    public decimal Net { get; private set; }
    public decimal Tax { get; private set; }
    public decimal Gross { get; private set; }

    private OrderLine() { } // ← EF

    public OrderLine(Guid orderId, string sku, string name, int qty, decimal unitPrice, int taxRate, decimal net, decimal tax, decimal gross)
    { OrderId = orderId; Sku = sku; Name = name; Quantity = qty; UnitPrice = unitPrice; TaxRateBasisPoints = taxRate; Net = net; Tax = tax; Gross = gross; }
}
