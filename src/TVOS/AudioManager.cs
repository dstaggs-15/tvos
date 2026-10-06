using System.Runtime.InteropServices;
namespace TVOS;
internal static class AudioManager {
 public static void ForceFullVolume(){try{var en=(IMMDeviceEnumerator)new MMDeviceEnumerator();en.GetDefaultAudioEndpoint(EDataFlow.eRender,ERole.eMultimedia,out var dev);var iid=typeof(IAudioEndpointVolume).GUID;dev.Activate(ref iid,CLSCTX.ALL,IntPtr.Zero,out var obj);var ep=(IAudioEndpointVolume)obj;ep.SetMute(false,Guid.Empty);ep.SetMasterVolumeLevelScalar(1f,Guid.Empty);Marshal.ReleaseComObject(ep);Marshal.ReleaseComObject(dev);Marshal.ReleaseComObject(en);}catch(Exception ex){CrashLog.Write(ex);}}
 enum EDataFlow{eRender,eCapture,eAll} enum ERole{eConsole,eMultimedia,eCommunications}
 [Flags] enum CLSCTX:uint{INPROC_SERVER=1,INPROC_HANDLER=2,LOCAL_SERVER=4,REMOTE_SERVER=16,ALL=23}
 [ComImport,Guid("BCDE0395-E52F-467C-8E3D-C4579291692E")] class MMDeviceEnumerator{}
 [ComImport,InterfaceType(ComInterfaceType.InterfaceIsIUnknown),Guid("A95664D2-9614-4F35-A746-DE8DB63617E6")] interface IMMDeviceEnumerator{int EnumAudioEndpoints(EDataFlow f,uint m,out IntPtr d);int GetDefaultAudioEndpoint(EDataFlow f,ERole r,out IMMDevice e);}
 [ComImport,InterfaceType(ComInterfaceType.InterfaceIsIUnknown),Guid("D666063F-1587-4E43-81F1-B948E807363F")] interface IMMDevice{int Activate(ref Guid iid,CLSCTX c,IntPtr p,[MarshalAs(UnmanagedType.IUnknown)]out object o);}
 [ComImport,InterfaceType(ComInterfaceType.InterfaceIsIUnknown),Guid("5CDF2C82-841E-4546-9722-0CF74078229A")] interface IAudioEndpointVolume{
  int RegisterControlChangeNotify(IntPtr n);int UnregisterControlChangeNotify(IntPtr n);int GetChannelCount(out uint c);int SetMasterVolumeLevel(float l,Guid g);int SetMasterVolumeLevelScalar(float l,Guid g);int GetMasterVolumeLevel(out float l);int GetMasterVolumeLevelScalar(out float l);int SetChannelVolumeLevel(uint c,float l,Guid g);int SetChannelVolumeLevelScalar(uint c,float l,Guid g);int GetChannelVolumeLevel(uint c,out float l);int GetChannelVolumeLevelScalar(uint c,out float l);int SetMute([MarshalAs(UnmanagedType.Bool)]bool m,Guid g);int GetMute(out bool m);
 }
}