using Microsoft.EntityFrameworkCore;
using Shop.Application;
using Shop.Infrastructure;
using MediatR;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddDbContext<ShopDbContext>(opt =>
    opt.UseSqlServer(builder.Configuration.GetConnectionString("ShopDb") ??
                     "Server=(localdb)\\MSSQLLocalDB;Database=ShopDb;Trusted_Connection=True;TrustServerCertificate=True"));

builder.Services.AddMediatR(typeof(Commands.AddItem));
builder.Services.AddScoped<IShopRepository, EfShopRepository>();

builder.Services.AddHttpClient<IOrdersClient, OrdersClient>(http =>
{
    http.BaseAddress = new Uri(builder.Configuration["Orders:BaseUrl"] ?? "https://localhost:5003/");
});
builder.Services.AddHttpClient<IPaymentsClient, PaymentsClient>(http =>
{
    http.BaseAddress = new Uri(builder.Configuration["Payments:BaseUrl"] ?? "https://localhost:5002/");
});

var app = builder.Build();

if (app.Environment.IsDevelopment()) { app.UseSwagger(); app.UseSwaggerUI(); }

app.MapControllers();

// Seed some products
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<ShopDbContext>();
    db.Database.EnsureCreated();
    if (!db.Products.Any())
    {
        db.Products.AddRange(
            new Shop.Domain.Product("SKU-001", "LEGO Classic Bricks", 19.99m),
            new Shop.Domain.Product("SKU-002", "Wireless Mouse", 24.50m),
            new Shop.Domain.Product("SKU-003", "Mechanical Keyboard", 89.00m),
            new Shop.Domain.Product("SKU-004", "Noise-Canceling Headphones", 129.99m)
        );
        db.SaveChanges();
    }
}

app.Run();
