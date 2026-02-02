using System.Text;

namespace Awk.Services;

internal sealed record OAuthAuthorizationRequest(Uri Url, string State, string CodeVerifier);

internal static class OAuthAuthorizationRequestFactory
{
    internal static OAuthAuthorizationRequest CreateAwork(string clientId, string redirectUri, string scopes, Uri authorizationEndpoint)
    {
        var pkce = OAuthPkce.Generate();
        var state = Guid.NewGuid().ToString("N");
        var normalizedRedirectUri = NormalizeRedirectUri(redirectUri);

        var query = new Dictionary<string, string?>
        {
            ["response_type"] = "code",
            ["client_id"] = clientId,
            ["redirect_uri"] = normalizedRedirectUri,
            ["scope"] = scopes,
            ["code_challenge"] = pkce.Challenge,
            ["code_challenge_method"] = "S256",
            ["state"] = state,
        };

        var queryString = BuildQuery(query);
        var baseUri = authorizationEndpoint.ToString();
        var separator = baseUri.Contains('?', StringComparison.Ordinal) ? "&" : "?";
        var url = new Uri($"{baseUri}{separator}{queryString}");

        return new OAuthAuthorizationRequest(url, state, pkce.Verifier);
    }

    private static string BuildQuery(Dictionary<string, string?> values)
    {
        var sb = new StringBuilder();
        foreach (var (key, value) in values)
        {
            if (string.IsNullOrWhiteSpace(value)) continue;
            if (sb.Length > 0) sb.Append('&');
            sb.Append(Uri.EscapeDataString(key));
            sb.Append('=');
            if (string.Equals(key, "redirect_uri", StringComparison.Ordinal))
            {
                sb.Append(value);
            }
            else
            {
                sb.Append(Uri.EscapeDataString(value));
            }
        }
        return sb.ToString();
    }

    internal static string NormalizeRedirectUri(string value)
    {
        var current = value.Trim();
        if (string.IsNullOrWhiteSpace(current)) return current;

        for (var i = 0; i < 5; i++)
        {
            if (Uri.TryCreate(current, UriKind.Absolute, out _))
            {
                return current;
            }

            if (!current.Contains('%', StringComparison.Ordinal))
            {
                return current;
            }

            var unescaped = Uri.UnescapeDataString(current);
            if (string.Equals(unescaped, current, StringComparison.Ordinal))
            {
                return current;
            }

            current = unescaped;
        }

        return current;
    }
}
