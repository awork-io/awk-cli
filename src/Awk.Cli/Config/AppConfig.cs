namespace Awk.Config;

internal sealed record AppConfig(
    string BaseUrl,
    string Token,
    string EnvFile
)
{
    internal static AppConfig Default(string envFile) => new(
        BaseUrl: "https://api.awork.com/api/v1",
        Token: string.Empty,
        EnvFile: envFile
    );
}
