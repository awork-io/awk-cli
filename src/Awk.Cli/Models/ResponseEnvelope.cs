namespace Awk.Models;

public sealed record ResponseEnvelope<T>(
    int StatusCode,
    string? TraceId,
    T Response
);

public static class ResponseEnvelope
{
    internal static ResponseEnvelope<T> Ok<T>(int statusCode, string? traceId, T response) =>
        new(statusCode, traceId, response);
}
