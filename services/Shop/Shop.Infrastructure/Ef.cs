using Microsoft.EntityFrameworkCore;
using Shop.Domain;
using Shop.Application;

namespace Shop.Infrastructure;

public class ShopDbContext : DbContext
{
    public ShopDbContext(DbContextOptions<ShopDbContext> options) : base(options) { }
    public DbSet<Basket> Baskets => Set<Basket>();
    public DbSet<BasketItem> BasketItems => Set<BasketItem>();
    public DbSet<Product> Products => Set<Product>();

    protected override void OnModelCreating(ModelBuilder b)
    {
        b.Entity<Basket>().HasKey(x => x.Id);
        b.Entity<Basket>().HasMany(x => x.Items).WithOne().HasForeignKey("BasketId");
        b.Entity<BasketItem>().HasKey(x => x.Id);

        b.Entity<Product>().HasKey(x => x.Id);
        b.Entity<Product>().Property(x => x.Sku).HasMaxLength(32);
    }
}

public sealed class EfShopRepository : IShopRepository
{
    private readonly ShopDbContext _db;
    public EfShopRepository(ShopDbContext db) => _db = db;

    public async Task<Basket> GetOrCreateBasketAsync(string userId, CancellationToken ct)
    {
        var b = await _db.Baskets.Include(x => x.Items).FirstOrDefaultAsync(x => x.UserId == userId, ct);
        if (b is null)
        {
            b = new Basket(userId);
            _db.Baskets.Add(b);
            await _db.SaveChangesAsync(ct);
        }
        return b;
    }

    public IQueryable<Product> Products => _db.Products;

    public Task SaveChangesAsync(CancellationToken ct) => _db.SaveChangesAsync(ct);
}
