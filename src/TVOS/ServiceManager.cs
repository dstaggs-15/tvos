using System.Text.Json;
namespace TVOS;

public sealed class ServiceManager
{
    static readonly JsonSerializerOptions J = new() { WriteIndented = true, PropertyNameCaseInsensitive = true };

    public ServicesConfig Load()
    {
        Ensure();
        var config = JsonSerializer.Deserialize<ServicesConfig>(File.ReadAllText(AppPaths.Services), J) ?? new();
        RepairBuiltInIcons(config);
        return config;
    }

    public void Save(ServicesConfig c)
    {
        Directory.CreateDirectory(AppPaths.Config);
        c.Services = c.Services.OrderBy(x => x.Order).ToList();
        File.WriteAllText(AppPaths.Services, JsonSerializer.Serialize(c, J));
    }

    public AppSettings LoadSettings()
    {
        Ensure();
        var s = JsonSerializer.Deserialize<AppSettings>(File.ReadAllText(AppPaths.Settings), J) ?? new();
        if (string.IsNullOrWhiteSpace(s.RemoteToken))
        {
            s.RemoteToken = Convert.ToHexString(System.Security.Cryptography.RandomNumberGenerator.GetBytes(18)).ToLowerInvariant();
            File.WriteAllText(AppPaths.Settings, JsonSerializer.Serialize(s, J));
        }
        return s;
    }

    static void RepairBuiltInIcons(ServicesConfig config)
    {
        bool changed = false;
        foreach (var service in config.Services)
        {
            if (string.IsNullOrWhiteSpace(service.Icon) && !string.IsNullOrWhiteSpace(service.Id))
            {
                var relative = $"assets/services/{service.Id}.svg";
                var installed = Path.Combine(AppPaths.Root, "assets", "services", $"{service.Id}.svg");
                if (File.Exists(installed))
                {
                    service.Icon = relative;
                    changed = true;
                }
            }
        }

        if (changed)
            File.WriteAllText(AppPaths.Services, JsonSerializer.Serialize(config, J));
    }

    static void Ensure()
    {
        Directory.CreateDirectory(AppPaths.Config);
        Copy("services.default.json", AppPaths.Services);
        Copy("settings.default.json", AppPaths.Settings);
    }

    static void Copy(string n, string d)
    {
        if (File.Exists(d)) return;
        var s = Path.Combine(AppPaths.Config, n);
        if (File.Exists(s)) File.Copy(s, d);
    }
}
