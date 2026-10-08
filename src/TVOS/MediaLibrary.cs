using System.Diagnostics;

namespace TVOS;

public sealed class MediaLibrary
{
    static readonly HashSet<string> Extensions = new(StringComparer.OrdinalIgnoreCase)
    { ".mkv", ".mp4", ".m4v", ".avi", ".mov", ".wmv", ".mpg", ".mpeg", ".ts", ".m2ts", ".vob" };

    public MediaLibrary() => Directory.CreateDirectory(AppPaths.Media);

    public IReadOnlyList<MediaItem> Scan()
    {
        Directory.CreateDirectory(AppPaths.Media);
        return Directory.EnumerateFiles(AppPaths.Media, "*.*", SearchOption.AllDirectories)
            .Where(p => Extensions.Contains(Path.GetExtension(p)))
            .Select(p => new MediaItem {
                Id = Convert.ToHexString(System.Security.Cryptography.SHA256.HashData(System.Text.Encoding.UTF8.GetBytes(p))).ToLowerInvariant()[..16],
                Title = CleanTitle(Path.GetFileNameWithoutExtension(p)),
                Path = p
            })
            .OrderBy(x => x.Title, StringComparer.CurrentCultureIgnoreCase)
            .ToList();
    }

    public bool Play(string id)
    {
        var item = Scan().FirstOrDefault(x => x.Id == id);
        if (item == null || !File.Exists(item.Path)) return false;
        Process.Start(new ProcessStartInfo { FileName = item.Path, UseShellExecute = true });
        return true;
    }

    static string CleanTitle(string name) => name.Replace('.', ' ').Replace('_', ' ').Trim();
}
