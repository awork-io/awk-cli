using Awk.Cli;
using Awk.Config;
using Awk.Generated;
using Awk.Services;
using Spectre.Console.Cli;

namespace Awk.Commands;

internal abstract class CommandBase<TSettings> : AsyncCommand<TSettings> where TSettings : BaseSettings
{
    protected AworkClient CreateClient(TSettings settings)
    {
        var config = ConfigLoader.Load(settings.EnvFile, settings.BaseUrl);
        return new AworkClientFactory().Create(config);
    }

    protected int Output(object payload) => JsonConsole.Write(payload);

    protected int OutputError(Exception ex) => JsonConsole.WriteError(ex);
}
