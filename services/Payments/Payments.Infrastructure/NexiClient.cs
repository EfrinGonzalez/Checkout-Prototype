using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using Microsoft.Extensions.Configuration;
using Payments.Application;

namespace Payments.Infrastructure;

public sealed class NexiClient : INexiClient
{
    private readonly HttpClient _http; private readonly IConfiguration _cfg;
    public NexiClient(HttpClient http, IConfiguration cfg){ _http=http; _cfg=cfg; }

    public async Task<(string paymentId, string hostedUrl)> CreatePaymentAsync(string orderId, string currency, int amountMinor, string returnUrl, string cancelUrl, CancellationToken ct)
    {
        var baseUrl = _cfg["Nexi:ApiBase"] ?? "https://test.api.dibspayment.eu";
        var secret = _cfg["Nexi:SecretKey"] ?? "REPLACE_ME";
        using var req = new HttpRequestMessage(HttpMethod.Post, $"{baseUrl}/v1/payments");
        req.Headers.TryAddWithoutValidation("Authorization", secret);
        req.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
        var body = new { order = new { amount = amountMinor, currency, reference = orderId }, checkout = new { integrationType = "HostedPaymentPage", returnUrl, cancelUrl } };
        req.Content = new StringContent(JsonSerializer.Serialize(body), Encoding.UTF8, "application/*+json");
        var res = await _http.SendAsync(req, ct); res.EnsureSuccessStatusCode();
        var json = await res.Content.ReadAsStringAsync(ct);
        using var doc = JsonDocument.Parse(json);
        var pid = doc.RootElement.GetProperty("paymentId").GetString() ?? throw new InvalidOperationException("No paymentId");
        var url = doc.RootElement.TryGetProperty("hostedPaymentPageUrl", out var v) ? v.GetString() : "";
        return (pid, url ?? "");
    }

    public async Task CaptureAsync(string paymentId, int amountMinor, CancellationToken ct)
    {
        var baseUrl = _cfg["Nexi:ApiBase"] ?? "https://test.api.dibspayment.eu";
        var secret = _cfg["Nexi:SecretKey"] ?? "REPLACE_ME";
        using var req = new HttpRequestMessage(HttpMethod.Post, $"{baseUrl}/v1/payments/{paymentId}/charges");
        req.Headers.TryAddWithoutValidation("Authorization", secret);
        req.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
        req.Content = new StringContent(JsonSerializer.Serialize(new { amount = amountMinor }), Encoding.UTF8, "application/*+json");
        var res = await _http.SendAsync(req, ct); res.EnsureSuccessStatusCode();
    }
}
