using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Text.Json;

namespace TVOS;

public sealed class RemoteServer : IDisposable
{
    private readonly TcpListener listener;
    private readonly CancellationTokenSource stop = new();
    private readonly AppSettings settings;
    private readonly ServiceManager services;
    private readonly Action<string> launch;
    private readonly Action home;
    private readonly ArtworkManager artwork = new();

    public RemoteServer(AppSettings settings, ServiceManager services, Action<string> launch, Action home)
    {
        this.settings = settings; this.services = services; this.launch = launch; this.home = home;
        listener = new TcpListener(IPAddress.Any, settings.RemotePort);
    }

    public void Start(){ listener.Start(); _ = Task.Run(AcceptLoop); }

    public string Url()
    {
        var ip = Dns.GetHostEntry(Dns.GetHostName()).AddressList.FirstOrDefault(x => x.AddressFamily == AddressFamily.InterNetwork && !IPAddress.IsLoopback(x));
        return $"http://{ip ?? IPAddress.Loopback}:{settings.RemotePort}/remote/?token={settings.RemoteToken}";
    }

    private async Task AcceptLoop()
    {
        while (!stop.IsCancellationRequested)
        {
            try { var client = await listener.AcceptTcpClientAsync(stop.Token); _ = Task.Run(() => Handle(client)); }
            catch (OperationCanceledException) { break; }
            catch { await Task.Delay(100); }
        }
    }

    private async Task Handle(TcpClient client)
    {
        using (client)
        using (var stream = client.GetStream())
        using (var reader = new StreamReader(stream, Encoding.UTF8, false, 8192, true))
        {
            var request = await reader.ReadLineAsync(); if (request == null) return;
            var requestParts = request.Split(' '); if (requestParts.Length < 2) return;
            var headers = new Dictionary<string,string>(StringComparer.OrdinalIgnoreCase);
            string? line;
            while (!string.IsNullOrEmpty(line = await reader.ReadLineAsync())) { var i=line.IndexOf(':'); if(i>0) headers[line[..i].Trim()]=line[(i+1)..].Trim(); }
            var length = headers.TryGetValue("Content-Length",out var rawLength) && int.TryParse(rawLength,out var parsed) ? parsed : 0;
            var body = "";
            if(length>0){var chars=new char[length];var read=0;while(read<length){var got=await reader.ReadAsync(chars,read,length-read);if(got==0)break;read+=got;}body=new string(chars,0,read);}

            var uri = new Uri("http://localhost"+requestParts[1]);
            var token = Query(uri.Query).GetValueOrDefault("token","");
            if(uri.AbsolutePath.StartsWith("/api/") && token!=settings.RemoteToken){await Reply(stream,403,"text/plain","Forbidden");return;}

            if(requestParts[0]=="GET" && uri.AbsolutePath.StartsWith("/remote"))
            {
                var rel=uri.AbsolutePath is "/remote" or "/remote/" ? "index.html" : uri.AbsolutePath["/remote/".Length..];
                var file=Path.GetFullPath(Path.Combine(AppPaths.Remote,rel));
                if(!file.StartsWith(Path.GetFullPath(AppPaths.Remote),StringComparison.OrdinalIgnoreCase)||!File.Exists(file)){await Reply(stream,404,"text/plain","Not found");return;}
                await ReplyBytes(stream,200,Mime(file),await File.ReadAllBytesAsync(file));return;
            }

            if(requestParts[0]=="GET" && uri.AbsolutePath=="/api/state")
            {
                await Reply(stream,200,"application/json",JsonSerializer.Serialize(new{services=services.Load().Services,theme=ThemeManager.Resolve(settings.Theme)}));return;
            }

            if(requestParts[0]=="POST" && uri.AbsolutePath=="/api/action")
            {
                using var doc=JsonDocument.Parse(body);var root=doc.RootElement;var action=root.GetProperty("action").GetString()??"";
                switch(action)
                {
                    case "home": home(); break;
                    case "up": InputManager.Key(0x26); break;
                    case "down": InputManager.Key(0x28); break;
                    case "left": InputManager.Key(0x25); break;
                    case "right": InputManager.Key(0x27); break;
                    case "ok": InputManager.Key(0x0D); break;
                    case "back": InputManager.Key(0xA6); break;
                    case "playpause": InputManager.Key(0xB3); break;
                    case "sleep": PowerManager.Sleep(); break;
                    case "text": if(root.TryGetProperty("text",out var t))InputManager.Text(t.GetString()??""); break;
                    case "launch": if(root.TryGetProperty("url",out var u))launch(u.GetString()??""); break;
                    case "artwork":
                        if(root.TryGetProperty("id",out var id))
                        {
                            var cfg=services.Load();var svc=cfg.Services.FirstOrDefault(x=>x.Id==id.GetString());
                            if(svc!=null){var found=await artwork.DiscoverAsync(svc);if(!string.IsNullOrWhiteSpace(found)){svc.Icon=found;services.Save(cfg);}}
                        }
                        break;
                }
                await Reply(stream,200,"application/json","{\"ok\":true}");return;
            }

            if(requestParts[0]=="POST" && uri.AbsolutePath=="/api/services")
            {
                var cfg=JsonSerializer.Deserialize<ServicesConfig>(body,new JsonSerializerOptions{PropertyNameCaseInsensitive=true});
                if(cfg==null){await Reply(stream,400,"text/plain","Bad request");return;}
                for(var i=0;i<cfg.Services.Count;i++){cfg.Services[i].Order=i;if(string.IsNullOrWhiteSpace(cfg.Services[i].Id))cfg.Services[i].Id=Guid.NewGuid().ToString("N")[..10];}
                services.Save(cfg);await Reply(stream,200,"application/json","{\"ok\":true}");return;
            }
            await Reply(stream,404,"text/plain","Not found");
        }
    }

    private static Dictionary<string,string> Query(string q)=>q.TrimStart('?').Split('&',StringSplitOptions.RemoveEmptyEntries).Select(x=>x.Split('=',2)).ToDictionary(x=>Uri.UnescapeDataString(x[0]),x=>x.Length>1?Uri.UnescapeDataString(x[1]):"");
    private static string Mime(string f)=>Path.GetExtension(f).ToLowerInvariant() switch {".html"=>"text/html; charset=utf-8",".css"=>"text/css; charset=utf-8",".js"=>"text/javascript; charset=utf-8",".svg"=>"image/svg+xml",".png"=>"image/png",".jpg" or ".jpeg"=>"image/jpeg",".webp"=>"image/webp",".ico"=>"image/x-icon",_=>"application/octet-stream"};
    private static Task Reply(NetworkStream s,int code,string type,string body)=>ReplyBytes(s,code,type,Encoding.UTF8.GetBytes(body));
    private static async Task ReplyBytes(NetworkStream s,int code,string type,byte[] body){var reason=code switch{200=>"OK",400=>"Bad Request",403=>"Forbidden",404=>"Not Found",_=>"Error"};var head=Encoding.ASCII.GetBytes($"HTTP/1.1 {code} {reason}\r\nContent-Type: {type}\r\nContent-Length: {body.Length}\r\nCache-Control: no-store\r\nX-Content-Type-Options: nosniff\r\nConnection: close\r\n\r\n");await s.WriteAsync(head);await s.WriteAsync(body);}
    public void Dispose(){stop.Cancel();listener.Stop();}
}