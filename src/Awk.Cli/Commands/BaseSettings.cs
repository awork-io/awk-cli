using Spectre.Console.Cli;

namespace Awk.Commands;

internal class BaseSettings : CommandSettings
{
    [CommandOption("--env <PATH>")]
    public string? EnvFile { get; init; }

    [CommandOption("--base-url <URL>")]
    public string? BaseUrl { get; init; }
}
