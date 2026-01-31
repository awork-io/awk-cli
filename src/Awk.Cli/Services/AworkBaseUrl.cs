using Awk.Config;

namespace Awk.Services;

internal static class AworkBaseUrl
{
    internal const string Default = AppConfig.DefaultBaseUrl;

    internal static string Resolve()
    {
#if DEBUG
        var overrideUrl = Environment.GetEnvironmentVariable("AWK_TEST_BASE_URL")
            ?? Environment.GetEnvironmentVariable("AWORK_TEST_BASE_URL");
        if (!string.IsNullOrWhiteSpace(overrideUrl))
        {
            return overrideUrl.Trim();
        }
#endif

        return Default;
    }
}
