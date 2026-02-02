using System.Text.Encodings.Web;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Awk.Cli;

internal static class JsonConsole
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
        WriteIndented = true,
        Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping
    };

    internal static int Write<T>(T payload)
    {
        var json = JsonSerializer.Serialize(payload, JsonOptions);
        Console.WriteLine(json);
        return 0;
    }

    internal static int WriteError(Exception ex)
    {
        var error = new
        {
            statusCode = 0,
            traceId = (string?)null,
            response = new
            {
                error = ex.Message,
                type = ex.GetType().Name
            }
        };
        return Write(error);
    }
}
