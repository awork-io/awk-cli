using Awk.Services;

namespace Awk.Cli.Tests;

public sealed class OAuthAuthorizationRequestFactoryTests
{
    [Theory]
    [InlineData("http://localhost:8400/oauth/callback", "http://localhost:8400/oauth/callback")]
    [InlineData("http%3A%2F%2Flocalhost%3A8400%2Foauth%2Fcallback", "http://localhost:8400/oauth/callback")]
    [InlineData("http%253A%252F%252Flocalhost%253A8400%252Foauth%252Fcallback", "http://localhost:8400/oauth/callback")]
    public void NormalizeRedirectUri_DecodesUntilStable(string input, string expected)
    {
        var normalized = OAuthAuthorizationRequestFactory.NormalizeRedirectUri(input);
        Assert.Equal(expected, normalized);
    }

    [Fact]
    public void CreateAwork_UsesNormalizedRedirectUriInQuery()
    {
        var encoded = "http%3A%2F%2Flocalhost%3A8400%2Foauth%2Fcallback";
        var request = OAuthAuthorizationRequestFactory.CreateAwork(
            "client-id",
            encoded,
            "full_access offline_access",
            new Uri("https://example.com/authorize"));

        var redirectUri = GetQueryValue(request.Url, "redirect_uri");
        Assert.Equal("http://localhost:8400/oauth/callback", redirectUri);
    }

    private static string GetQueryValue(Uri url, string key)
    {
        var query = url.Query.TrimStart('?');
        foreach (var part in query.Split('&', StringSplitOptions.RemoveEmptyEntries))
        {
            var pair = part.Split('=', 2);
            if (pair.Length != 2) continue;
            var pairKey = Uri.UnescapeDataString(pair[0]);
            if (!string.Equals(pairKey, key, StringComparison.Ordinal)) continue;
            return Uri.UnescapeDataString(pair[1]);
        }

        return string.Empty;
    }
}
