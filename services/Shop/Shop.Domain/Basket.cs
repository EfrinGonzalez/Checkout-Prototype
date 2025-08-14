namespace Shop.Domain;

public class Basket
{
    public Guid Id { get; private set; } = Guid.NewGuid();
    public string UserId { get; private set; } = null!;
    public DateTime UpdatedAtUtc { get; private set; } = DateTime.UtcNow;
    public List<BasketItem> Items { get; private set; } = new();
    public Basket(string userId) { UserId = userId; }
    public void AddItem(Guid productId, string sku, string name, decimal unitPrice, int qty, int taxRateBasisPoints)
    { var existing = Items.FirstOrDefault(i => i.ProductId == productId); if (existing is null) Items.Add(new BasketItem(productId, sku, name, unitPrice, qty, taxRateBasisPoints)); else existing.Increase(qty); UpdatedAtUtc = DateTime.UtcNow; }
    public void RemoveItem(Guid productId){ Items.RemoveAll(i => i.ProductId == productId); UpdatedAtUtc = DateTime.UtcNow; }
    public void Clear(){ Items.Clear(); UpdatedAtUtc = DateTime.UtcNow; }
}
public class BasketItem
{
    public Guid Id { get; private set; } = Guid.NewGuid();
    public Guid BasketId { get; private set; }
    public Guid ProductId { get; private set; }
    public string Sku { get; private set; } = null!;
    public string Name { get; private set; } = null!;
    public decimal UnitPrice { get; private set; }
    public int Quantity { get; private set; }
    public int TaxRateBasisPoints { get; private set; }
    private BasketItem() { } // ← EF needs this
    public BasketItem(Guid productId, string sku, string name, decimal unitPrice, int qty, int taxRate){ ProductId = productId; Sku = sku; Name = name; UnitPrice = unitPrice; Quantity = qty; TaxRateBasisPoints = taxRate; }
    public void Increase(int by) => Quantity += by;
}
public class Product
{
    public Guid Id { get; private set; } = Guid.NewGuid();
    public string Sku { get; private set; } = null!;
    public string Name { get; private set; } = null!;
    public decimal Price { get; private set; }
    public Product(string sku, string name, decimal price) { Sku = sku; Name = name; Price = price; }
}
