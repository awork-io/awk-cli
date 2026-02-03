using Awk.Cli;
using Awk.Config;
using Awk.Generated;
using Awk.Models;
using Awk.Services;
using Spectre.Console.Cli;

namespace Awk.Commands;

internal abstract class CommandBase<TSettings> : AsyncCommand<TSettings> where TSettings : BaseSettings
{
    private string? _outputFormat;

    protected async Task<AworkClient> CreateClient(TSettings settings, CancellationToken cancellationToken)
    {
        _outputFormat = settings.Output;

        var loaded = await ConfigLoader.Load(
            settings.EnvFile,
            settings.Token,
            settings.ConfigPath,
            cancellationToken);

        var authMode = AuthModeParser.Parse(settings.AuthMode);
        var auth = await AuthResolver.Resolve(
            loaded.BaseConfig,
            loaded.EffectiveConfig,
            authMode,
            cancellationToken);

        if (auth.UpdatedConfig is not null)
        {
            await ConfigLoader.SaveUserConfig(auth.UpdatedConfig, loaded.ConfigPath, cancellationToken);
        }

        return new AworkClientFactory().Create(auth.Token);
    }

    protected int Output(object payload)
    {
        if (string.Equals(_outputFormat, "table", StringComparison.OrdinalIgnoreCase))
        {
            var type = payload?.GetType();
            if (type?.IsGenericType == true && type.GetGenericTypeDefinition() == typeof(ResponseEnvelope<>))
            {
                var statusCode = (int)(type.GetProperty("StatusCode")?.GetValue(payload) ?? 0);
                var traceId = type.GetProperty("TraceId")?.GetValue(payload) as string;
                var response = type.GetProperty("Response")?.GetValue(payload);
                return TableConsole.Write(new ResponseEnvelope<object?>(statusCode, traceId, response));
            }
        }

        return JsonConsole.Write(payload);
    }

    protected int OutputError(Exception ex) => JsonConsole.WriteError(ex);
}
