using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using Awk.Models;

namespace Awk.Generated;

public sealed partial class AworkClient
{
    private readonly HttpClient _http;
    private readonly string _baseUrl;
    private readonly JsonSerializerOptions _jsonOptions;

    public AworkClient(HttpClient http, string baseUrl)
    {
        _http = http;
        _baseUrl = baseUrl.TrimEnd('/');
        _jsonOptions = new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            DefaultIgnoreCondition = System.Text.Json.Serialization.JsonIgnoreCondition.WhenWritingNull
        };
    }

    public async Task<ResponseEnvelope<object?>> Call(
        string method,
        string path,
        Dictionary<string, object?>? query,
        object? body,
        string? contentType,
        CancellationToken cancellationToken)
    {
        var url = BuildUrl(path, query);
        using var request = new HttpRequestMessage(new HttpMethod(method), url);

        if (body is not null)
        {
            if (body is HttpContent content)
            {
                request.Content = content;
            }
            else
            {
                var json = JsonSerializer.Serialize(body, _jsonOptions);
                request.Content = new StringContent(json, Encoding.UTF8, contentType ?? "application/json");
            }
        }

        using var response = await _http.SendAsync(request, cancellationToken);
        var statusCode = (int)response.StatusCode;
        var traceId = ExtractTraceId(response.Headers);
        object? payload = null;

        if (response.Content is not null)
        {
            var raw = await response.Content.ReadAsStringAsync(cancellationToken);
            if (!string.IsNullOrWhiteSpace(raw))
            {
                payload = TryParseJson(raw) ?? raw;
            }
        }

        return new ResponseEnvelope<object?>(statusCode, traceId, payload);
    }

    private string BuildUrl(string path, Dictionary<string, object?>? query)
    {
        var url = path.StartsWith("http", StringComparison.OrdinalIgnoreCase)
            ? path
            : _baseUrl + path;

        if (query is null || query.Count == 0) return url;

        var parts = new List<string>();
        foreach (var (key, value) in query)
        {
            if (value is null) continue;
            if (value is string s)
            {
                parts.Add(Encode(key, s));
                continue;
            }

            if (value is System.Collections.IEnumerable enumerable)
            {
                foreach (var item in enumerable)
                {
                    if (item is null) continue;
                    parts.Add(Encode(key, FormatQueryValue(item)));
                }
                continue;
            }

            parts.Add(Encode(key, FormatQueryValue(value)));
        }

        if (parts.Count == 0) return url;
        return url + "?" + string.Join("&", parts);
    }

    private static string Encode(string key, string value) =>
        Uri.EscapeDataString(key) + "=" + Uri.EscapeDataString(value);

    private static string FormatQueryValue(object value)
    {
        return value switch
        {
            bool b => b ? "true" : "false",
            DateTimeOffset dto => dto.ToString("o"),
            DateOnly date => date.ToString("O"),
            _ => value.ToString() ?? string.Empty
        };
    }

    private static object? TryParseJson(string raw)
    {
        try
        {
            using var doc = JsonDocument.Parse(raw);
            return doc.RootElement.Clone();
        }
        catch
        {
            return null;
        }
    }

    private static string? ExtractTraceId(HttpResponseHeaders? headers)
    {
        if (headers is null) return null;
        if (headers.TryGetValues("trace-id", out var traceId)) return traceId.FirstOrDefault();
        if (headers.TryGetValues("traceparent", out var traceParent)) return traceParent.FirstOrDefault();
        if (headers.TryGetValues("x-correlation-id", out var correlation)) return correlation.FirstOrDefault();
        if (headers.TryGetValues("request-id", out var requestId)) return requestId.FirstOrDefault();
        return null;
    }

    private static string Escape(string value) => Uri.EscapeDataString(value);
}
