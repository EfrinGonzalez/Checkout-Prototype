using Microsoft.EntityFrameworkCore;
using Orders.Application;
using Orders.Infrastructure;
using MediatR;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddDbContext<OrdersDbContext>(opt =>
    opt.UseSqlServer(builder.Configuration.GetConnectionString("OrdersDb") ??
                     "Server=(localdb)\\MSSQLLocalDB;Database=OrdersDb;Trusted_Connection=True;TrustServerCertificate=True"));

builder.Services.AddMediatR(typeof(Commands.CreateOrder));
builder.Services.AddScoped<IOrdersRepository, EfOrdersRepository>();

var app = builder.Build();
if (app.Environment.IsDevelopment()) { app.UseSwagger(); app.UseSwaggerUI(); }
app.MapControllers();
app.Run();
