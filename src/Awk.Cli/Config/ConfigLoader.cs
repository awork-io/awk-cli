namespace Awk.Config;

internal static class ConfigLoader
{
    internal static AppConfig Load(string? envFileOverride)
    {
        var envFile = string.IsNullOrWhiteSpace(envFileOverride) ? ".env" : envFileOverride.Trim();
        var env = EnvFile.Load(envFile);

        // Base URL override for testing only (not documented)
        var baseUrl = FirstNonEmpty(
            Env("AWORK_BASE_URL"),
            env.GetValueOrDefault("AWORK_BASE_URL")
        ) ?? AppConfig.BaseUrl;

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

        return new AppConfig(token, envFile, baseUrl);
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
