using Microsoft.EntityFrameworkCore;
using Payments.Application;
using Payments.Domain;

namespace Payments.Infrastructure;

public class PaymentsDbContext : DbContext
{
    public PaymentsDbContext(DbContextOptions<PaymentsDbContext> options) : base(options) { }
    public DbSet<PaymentIntent> PaymentIntents => Set<PaymentIntent>();
    protected override void OnModelCreating(ModelBuilder b)
    {
        b.Entity<PaymentIntent>().HasKey(x => x.PaymentId);
        b.Entity<PaymentIntent>().Property(x => x.Currency).HasMaxLength(3);
    }
}
public sealed class EfPaymentsRepository : IPaymentsRepository
{
    private readonly PaymentsDbContext _db;
    public EfPaymentsRepository(PaymentsDbContext db) => _db = db;
    public async Task AddAsync(PaymentIntent intent, CancellationToken ct){ _db.PaymentIntents.Add(intent); await _db.SaveChangesAsync(ct);}    
    public Task<PaymentIntent?> GetAsync(string paymentId, CancellationToken ct) => _db.PaymentIntents.FirstOrDefaultAsync(p => p.PaymentId == paymentId, ct);
    public Task SaveChangesAsync(CancellationToken ct) => _db.SaveChangesAsync(ct);
}
