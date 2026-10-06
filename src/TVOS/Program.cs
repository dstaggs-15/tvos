namespace TVOS;
internal static class Program {
 static Mutex? instanceMutex;
 [STAThread] static void Main() {
  instanceMutex=new Mutex(true,@"Local\BGFTOS_LivingRoom_Instance",out bool first);
  if(!first)return;
  ApplicationConfiguration.Initialize();
  Application.SetUnhandledExceptionMode(UnhandledExceptionMode.CatchException);
  Application.ThreadException+=(_,e)=>CrashLog.Write(e.Exception);
  AppDomain.CurrentDomain.UnhandledException+=(_,e)=>CrashLog.Write(e.ExceptionObject as Exception??new Exception("Unknown fatal error"));
  Application.Run(new LauncherForm());
  GC.KeepAlive(instanceMutex);
 }
}
internal static class CrashLog {
 public static void Write(Exception ex){try{Directory.CreateDirectory(AppPaths.Logs);File.AppendAllText(Path.Combine(AppPaths.Logs,"bgftos.log"),"["+DateTime.Now.ToString("O")+"] "+ex+"\r\n");}catch{}}
}