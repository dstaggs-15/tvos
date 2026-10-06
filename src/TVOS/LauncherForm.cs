using Microsoft.Web.WebView2.Core;
using Microsoft.Web.WebView2.WinForms;
using Microsoft.Win32;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Text.Json;
namespace TVOS;
public class LauncherForm:Form {
 readonly WebView2 webView=new(){Dock=DockStyle.Fill}; readonly ServiceManager serviceManager=new(); AppSettings settings=new(); RemoteServer? remote; System.Windows.Forms.Timer? chromeWatch; System.Windows.Forms.Timer? audioWatch;
 const int HotkeyHome=1001,WmHotkey=0x0312; const uint ModNoRepeat=0x4000,VkHome=0x24;
 [DllImport("user32.dll")]static extern bool RegisterHotKey(IntPtr h,int id,uint m,uint k);[DllImport("user32.dll")]static extern bool UnregisterHotKey(IntPtr h,int id);
 public LauncherForm(){Text="BGFTOS";FormBorderStyle=FormBorderStyle.None;StartPosition=FormStartPosition.Manual;WindowState=FormWindowState.Normal;TopMost=true;BackColor=Color.Black;Controls.Add(webView);Shown+=LauncherShown;FormClosed+=(_,_)=>{UnregisterHotKey(Handle,HotkeyHome);remote?.Dispose();};SystemEvents.PowerModeChanged+=PowerModeChanged;}
 protected override void OnHandleCreated(EventArgs e){base.OnHandleCreated(e);RegisterHotKey(Handle,HotkeyHome,ModNoRepeat,VkHome);}
 protected override void WndProc(ref Message m){if(m.Msg==WmHotkey&&m.WParam.ToInt32()==HotkeyHome){ReturnHome();return;}base.WndProc(ref m);}
 async void LauncherShown(object? s,EventArgs e){try{CoverScreen();settings=serviceManager.LoadSettings();AudioManager.ForceFullVolume();audioWatch=new(){Interval=60000};audioWatch.Tick+=(_,_)=>AudioManager.ForceFullVolume();audioWatch.Start();Directory.CreateDirectory(AppPaths.WebView);Directory.CreateDirectory(AppPaths.Logs);var env=await CoreWebView2Environment.CreateAsync(null,AppPaths.WebView);await webView.EnsureCoreWebView2Async(env);var bs=webView.CoreWebView2.Settings;bs.AreDefaultContextMenusEnabled=false;bs.AreDevToolsEnabled=false;bs.IsStatusBarEnabled=false;bs.AreBrowserAcceleratorKeysEnabled=false;bs.IsZoomControlEnabled=false;webView.CoreWebView2.WebMessageReceived+=WebMessageReceived;webView.CoreWebView2.NavigationCompleted+=(_,_)=>webView.Focus();webView.Source=new Uri(AppPaths.Launcher);remote=new(settings,serviceManager,u=>BeginInvoke(()=>Launch(u)),()=>BeginInvoke(ReturnHome));remote.Start();}catch(Exception ex){CrashLog.Write(ex);MessageBox.Show(ex.Message,"BGFTOS startup error");}}
 void PowerModeChanged(object s,PowerModeChangedEventArgs e){if(e.Mode==PowerModes.Resume){AudioManager.ForceFullVolume();BeginInvoke(ShowHome);}}
 void WebMessageReceived(object? s,CoreWebView2WebMessageReceivedEventArgs e){try{using var d=JsonDocument.Parse(e.WebMessageAsJson);var r=d.RootElement;var a=r.TryGetProperty("action",out var z)?z.GetString():"";if(a=="getState")SendState();else if(a=="launch"&&r.TryGetProperty("url",out var u))Launch(u.GetString()??"");else if(a=="sleep")PowerManager.Sleep();else if(a=="remoteInfo")Send(new{type="remoteInfo",url=remote?.Url()??"Starting..."});}catch(Exception ex){CrashLog.Write(ex);Send(new{type="error",message=ex.Message});}}
 void SendState()=>Send(new{type="state",services=serviceManager.Load().Services.Where(x=>x.Enabled).OrderBy(x=>x.Order),theme=ThemeManager.Resolve(settings.Theme)});
 void Send(object o){if(webView.CoreWebView2!=null)webView.CoreWebView2.PostWebMessageAsJson(JsonSerializer.Serialize(o));}
 void Launch(string url){if(!Uri.TryCreate(url,UriKind.Absolute,out var uri)||(uri.Scheme!="https"&&uri.Scheme!="http"))return;if(!File.Exists(settings.ChromeExe)){MessageBox.Show("Google Chrome was not found. BGFTOS expects Chrome at:\n"+settings.ChromeExe,"BGFTOS");return;}CloseStreamingWindows();Hide();try{AudioManager.ForceFullVolume();Process.Start(new ProcessStartInfo{FileName=settings.ChromeExe,UseShellExecute=true,Arguments="--profile-directory=\""+settings.ChromeProfile+"\" --app=\""+url+"\" --start-fullscreen --hide-scrollbars --no-first-run"});StartChromeWatch();}catch(Exception ex){CrashLog.Write(ex);ShowHome();MessageBox.Show(ex.Message,"BGFTOS could not open the service");}}
 void StartChromeWatch(){chromeWatch?.Stop();chromeWatch=new(){Interval=1500};var grace=0;chromeWatch.Tick+=(_,_)=>{grace++;bool visible=Process.GetProcessesByName("chrome").Any(p=>{try{p.Refresh();return p.MainWindowHandle!=IntPtr.Zero;}catch{return false;}});if(grace>3&&!visible){chromeWatch.Stop();ShowHome();}};chromeWatch.Start();}
 public void ReturnHome(){chromeWatch?.Stop();CloseStreamingWindows();ShowHome();}
 static void CloseStreamingWindows(){foreach(var p in Process.GetProcessesByName("chrome"))try{p.Refresh();if(p.MainWindowHandle!=IntPtr.Zero)p.CloseMainWindow();}catch{}}
 void ShowHome(){AudioManager.ForceFullVolume();if(!Visible)Show();CoverScreen();BringToFront();Activate();webView.Focus();SendState();}
 void CoverScreen(){Bounds=Screen.FromControl(this).Bounds;WindowState=FormWindowState.Normal;TopMost=true;}
 protected override void Dispose(bool d){if(d)SystemEvents.PowerModeChanged-=PowerModeChanged;base.Dispose(d);}
}