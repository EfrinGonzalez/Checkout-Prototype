using Microsoft.EntityFrameworkCore;
using Payments.Application;
using Payments.Infrastructure;
using MediatR;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddDbContext<PaymentsDbContext>(opt =>
    opt.UseSqlServer(builder.Configuration.GetConnectionString("PaymentsDb") ??
                     "Server=(localdb)\\MSSQLLocalDB;Database=PaymentsDb;Trusted_Connection=True;TrustServerCertificate=True"));

builder.Services.AddMediatR(typeof(Commands.InitiatePayment));
builder.Services.AddScoped<IPaymentsRepository, EfPaymentsRepository>();
builder.Services.AddHttpClient<INexiClient, NexiClient>();
builder.Services.AddHttpClient("Orders", http => { http.BaseAddress = new Uri(builder.Configuration["Orders:BaseUrl"] ?? "https://localhost:5003/"); });

var app = builder.Build();
if (app.Environment.IsDevelopment()) { app.UseSwagger(); app.UseSwaggerUI(); }
app.MapControllers();
app.Run();
