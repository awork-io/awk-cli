using System.Net.Http.Headers;
using Awk.Config;
using Awk.Generated;

namespace Awk.Services;

internal sealed class AworkClientFactory
{
    internal AworkClient Create(AppConfig config)
    {
        var http = new HttpClient
        {
            BaseAddress = new Uri(config.BaseUrl),
            Timeout = TimeSpan.FromSeconds(100)
        };
        http.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", config.Token);
        http.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
        http.DefaultRequestHeaders.UserAgent.ParseAdd("awork-cli/0.1");
        return new AworkClient(http, config.BaseUrl);
    }
}
