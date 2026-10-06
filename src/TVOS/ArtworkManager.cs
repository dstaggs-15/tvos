using System.Net;
using System.Text.RegularExpressions;

namespace TVOS;

public sealed class ArtworkManager
{
    private readonly HttpClient client = new(new HttpClientHandler { AutomaticDecompression = DecompressionMethods.All })
    {
        Timeout = TimeSpan.FromSeconds(10)
    };

    public ArtworkManager() => client.DefaultRequestHeaders.UserAgent.ParseAdd("Mozilla/5.0 BGFT-OS/1.0");

    public async Task<string> DiscoverAsync(ServiceConfig service)
    {
        Directory.CreateDirectory(AppPaths.Cache);
        if (!Uri.TryCreate(service.Url, UriKind.Absolute, out var page)) return "";

        try
        {
            var html = await client.GetStringAsync(page);
            var candidates = new List<string>();
            AddMatches(candidates, html, @"<meta[^>]+(?:property|name)=[""'](?:og:image|twitter:image)[""'][^>]+content=[""']([^""']+)", 1);
            AddMatches(candidates, html, @"<link[^>]+rel=[""'][^""']*(?:apple-touch-icon|icon)[^""']*[""'][^>]+href=[""']([^""']+)", 1);
            candidates.Add(new Uri(page, "/favicon.ico").ToString());

            foreach (var raw in candidates.Distinct())
            {
                if (!Uri.TryCreate(page, WebUtility.HtmlDecode(raw), out var uri)) continue;
                try
                {
                    using var response = await client.GetAsync(uri);
                    if (!response.IsSuccessStatusCode) continue;
                    var bytes = await response.Content.ReadAsByteArrayAsync();
                    if (bytes.Length < 100 || bytes.Length > 5_000_000) continue;
                    var media = response.Content.Headers.ContentType?.MediaType ?? "";
                    var ext = media switch { "image/png" => ".png", "image/jpeg" => ".jpg", "image/svg+xml" => ".svg", "image/webp" => ".webp", "image/x-icon" or "image/vnd.microsoft.icon" => ".ico", _ => Path.GetExtension(uri.AbsolutePath) };
                    if (string.IsNullOrWhiteSpace(ext) || ext.Length > 5) ext = ".img";
                    var file = Path.Combine(AppPaths.Cache, Safe(service.Id) + ext.ToLowerInvariant());
                    await File.WriteAllBytesAsync(file, bytes);
                    return file;
                }
                catch { }
            }
        }
        catch { }
        return "";
    }

    private static void AddMatches(List<string> list, string html, string pattern, int group)
    {
        foreach (Match m in Regex.Matches(html, pattern, RegexOptions.IgnoreCase | RegexOptions.Singleline))
            if (m.Success) list.Add(m.Groups[group].Value);
    }

    private static string Safe(string value) => string.Concat(value.Where(c => char.IsLetterOrDigit(c) || c is '-' or '_'));
}