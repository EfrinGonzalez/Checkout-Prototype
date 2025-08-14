using Microsoft.EntityFrameworkCore;
using Orders.Domain;
using Orders.Application;

namespace Orders.Infrastructure;

public class OrdersDbContext : DbContext
{
    public OrdersDbContext(DbContextOptions<OrdersDbContext> options) : base(options) { }
    public DbSet<Order> Orders => Set<Order>();
    public DbSet<OrderLine> OrderLines => Set<OrderLine>();
    protected override void OnModelCreating(ModelBuilder b)
    {
        b.Entity<Order>().HasKey(x => x.Id);
        b.Entity<Order>().Property(x => x.Currency).HasMaxLength(3);
        b.Entity<Order>().HasMany(x => x.Lines).WithOne().HasForeignKey(x => x.OrderId);
        b.Entity<OrderLine>().HasKey(x => x.Id);
    }
}
public sealed class EfOrdersRepository : IOrdersRepository
{
    private readonly OrdersDbContext _db;
    public EfOrdersRepository(OrdersDbContext db) => _db = db;
    public async Task AddAsync(Order order, CancellationToken ct) { _db.Orders.Add(order); await _db.SaveChangesAsync(ct); }
    public Task<Order?> GetAsync(Guid id, CancellationToken ct) => _db.Orders.Include(o => o.Lines).FirstOrDefaultAsync(o => o.Id == id, ct);
    public Task SaveChangesAsync(CancellationToken ct) => _db.SaveChangesAsync(ct);
}
