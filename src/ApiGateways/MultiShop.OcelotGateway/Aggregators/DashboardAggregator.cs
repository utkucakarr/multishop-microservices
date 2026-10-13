using System.Net;
using System.Text;
using System.Text.Json.Nodes;
using Ocelot.Middleware;
using Ocelot.Multiplexer;
using Ocelot.Responder;

namespace MultiShop.OcelotGateway.Aggregators;

/// <summary>
/// <c>GET /services/admin/dashboard</c>: beş servisin <c>api/statistics</c> yanıtını
/// <c>{ "catalog": {...}, "comment": {...}, ... }</c> biçiminde tek JSON'da birleştirir.
/// <list type="bullet">
/// <item>Yetki hatası (401/403; gateway'de ya da serviste) tüm isteğin sonucudur: kısmi veri dönmez.</item>
/// <item>Bir servis çökükse / hata dönerse yalnızca onun alanı <c>null</c> olur; diğerleri gösterilir.</item>
/// </list>
/// Ocelot'un hazır birleştiricisi yanıt gövdelerini olduğu gibi yan yana koyar; bir servis hata verirse bozuk JSON oluşabilir.
/// </summary>
public class DashboardAggregator : IDefinedAggregator
{
    private readonly IErrorsToHttpStatusCodeMapper _errorMapper;
    private readonly ILogger<DashboardAggregator> _logger;

    public DashboardAggregator(IErrorsToHttpStatusCodeMapper errorMapper, ILogger<DashboardAggregator> logger)
    {
        _errorMapper = errorMapper;
        _logger = logger;
    }

    public async Task<DownstreamResponse> Aggregate(List<HttpContext> responses)
    {
        var dashboard = new JsonObject();

        foreach (var context in responses)
        {
            var key = context.Items.DownstreamRoute().Key;
            var errors = context.Items.Errors();
            var response = context.Items.DownstreamResponse();

            // Ocelot bu alt isteği servise hiç göndermediyse (ör. token yok/geçersiz, scope yok) hata listesi doludur.
            var statusCode = errors.Count > 0
                ? (HttpStatusCode)_errorMapper.Map(errors)
                : response?.StatusCode ?? HttpStatusCode.BadGateway;

            if (statusCode is HttpStatusCode.Unauthorized or HttpStatusCode.Forbidden)
                return EmptyResponse(statusCode);

            if ((int)statusCode is >= 200 and < 300 && response is not null)
            {
                var body = await response.Content.ReadAsStringAsync();
                dashboard[key] = JsonNode.Parse(body);
                continue;
            }

            _logger.LogWarning("Dashboard: {Service} istatistikleri alınamadı ({StatusCode}); alan boş gösterilecek", key, (int)statusCode);
            dashboard[key] = null;
        }

        return new DownstreamResponse(
            new StringContent(dashboard.ToJsonString(), Encoding.UTF8, "application/json"),
            HttpStatusCode.OK,
            new List<KeyValuePair<string, IEnumerable<string>>>(),
            "OK");
    }

    private static DownstreamResponse EmptyResponse(HttpStatusCode statusCode)
        => new(new StringContent(string.Empty), statusCode, new List<KeyValuePair<string, IEnumerable<string>>>(), statusCode.ToString());
}
