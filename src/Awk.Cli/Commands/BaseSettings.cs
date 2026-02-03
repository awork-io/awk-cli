using Awk.Services;
using Spectre.Console;
using Spectre.Console.Cli;

namespace Awk.Commands;

internal class BaseSettings : CommandSettings
{
    [CommandOption("--env <PATH>")]
    public string? EnvFile { get; init; }

    [CommandOption("--token <TOKEN>")]
    public string? Token { get; init; }

    [CommandOption("--auth-mode <MODE>")]
    public string? AuthMode { get; init; }

    [CommandOption("--config <PATH>")]
    public string? ConfigPath { get; init; }

    [CommandOption("--select <FIELDS>")]
    public string? Select { get; init; }

    [CommandOption("--output <FORMAT>")]
    public string? Output { get; init; }

    public override ValidationResult Validate()
    {
        if (!AuthModeParser.IsValid(AuthMode))
        {
            return ValidationResult.Error("auth-mode must be auto|token|oauth");
        }

        if (!string.IsNullOrWhiteSpace(Output) &&
            !string.Equals(Output, "json", StringComparison.OrdinalIgnoreCase) &&
            !string.Equals(Output, "table", StringComparison.OrdinalIgnoreCase))
        {
            return ValidationResult.Error("output must be json|table");
        }

        return ValidationResult.Success();
    }
}
