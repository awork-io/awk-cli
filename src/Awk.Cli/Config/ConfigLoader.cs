namespace Awk.Config;

internal static class ConfigLoader
{
    internal static AppConfig Load(string? envFileOverride, string? baseUrlOverride)
    {
        var envFile = string.IsNullOrWhiteSpace(envFileOverride) ? ".env" : envFileOverride.Trim();
        var config = AppConfig.Default(envFile);

        var env = EnvFile.Load(envFile);

        var baseUrl = FirstNonEmpty(
            baseUrlOverride,
            Env("AWORK_BASE_URL"),
            Env("AWK_BASE_URL"),
            env.GetValueOrDefault("AWORK_BASE_URL"),
            env.GetValueOrDefault("AWK_BASE_URL"),
            config.BaseUrl
        );

        var token = FirstNonEmpty(
            Env("AWORK_TOKEN"),
            Env("AWK_TOKEN"),
            Env("AWORK_BEARER_TOKEN"),
            Env("BEARER_TOKEN"),
            env.GetValueOrDefault("AWORK_TOKEN"),
            env.GetValueOrDefault("AWK_TOKEN"),
            env.GetValueOrDefault("AWORK_BEARER_TOKEN"),
            env.GetValueOrDefault("BEARER_TOKEN")
        );

        if (string.IsNullOrWhiteSpace(token))
        {
            throw new InvalidOperationException($"No bearer token found. Provide it in {envFile} (AWORK_TOKEN or BEARER_TOKEN). ");
        }

        return new AppConfig(baseUrl ?? "https://api.awork.com/api/v1", token, envFile);
    }

    private static string? Env(string name) => Environment.GetEnvironmentVariable(name);

    private static string? FirstNonEmpty(params string?[] values)
    {
        foreach (var value in values)
        {
            if (!string.IsNullOrWhiteSpace(value)) return value.Trim();
        }

        return null;
    }
}
