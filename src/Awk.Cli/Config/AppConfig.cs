namespace Awk.Config;

internal sealed record AppConfig(
    string Token,
    string EnvFile,
    string ApiBaseUrl
)
{
    internal const string BaseUrl = "https://api.awork.com/api/v1";

    internal static AppConfig Default(string envFile) => new(
        Token: string.Empty,
        EnvFile: envFile,
        ApiBaseUrl: BaseUrl
    );
}
