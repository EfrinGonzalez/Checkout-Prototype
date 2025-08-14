using Shared.Contracts;
using Shop.Application;
using System.Net.Http.Json;

namespace Shop.Infrastructure;

public sealed class OrdersClient : IOrdersClient
{
    private readonly HttpClient _http;
    public OrdersClient(HttpClient http) => _http = http;

    public async Task<string> CreateOrderAsync(CreateOrderRequest request, CancellationToken ct)
    {
        var res = await _http.PostAsJsonAsync("api/orders", request, ct);
        res.EnsureSuccessStatusCode();
        var data = await res.Content.ReadFromJsonAsync<CreateOrderResponse>(cancellationToken: ct);
        return data!.OrderId;
    }
}

public sealed class PaymentsClient : IPaymentsClient
{
    private readonly HttpClient _http;
    public PaymentsClient(HttpClient http) => _http = http;

    public async Task<InitiatePaymentResponse> InitiateAsync(InitiatePaymentRequest request, CancellationToken ct)
    {
        var res = await _http.PostAsJsonAsync("api/payments/initiate", request, ct);
        res.EnsureSuccessStatusCode();
        return (await res.Content.ReadFromJsonAsync<InitiatePaymentResponse>(cancellationToken: ct))!;
    }
}
