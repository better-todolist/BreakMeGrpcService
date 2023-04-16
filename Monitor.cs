using BreakMe;
using BreakMeGrpcService.DataObj;
using System.Text;
using Vanara.PInvoke;

namespace BreakMeGrpcService
{

    public class Monitor
    {
        public Monitor() { }

        private WindowsInfo getNowWindows(ObserveMode mode)
        {
            var windows = User32.GetForegroundWindow();
            return new WindowsInfo(mode, windows);
        }

        public void StartObserve(InterruptData intp)
        {
            Task.Run(() =>
            {

            });
        }
    }


    public sealed class WindowsInfo : IEquatable<WindowsInfo>
    {
        private readonly ObserveMode Mode;
        public String WindowName { get; set; }
        public uint Tid { get; set; }
        public uint Pid { get; set; }

        public String Executable { get; set; }

        public WindowsInfo(ObserveMode mode, HWND handleWindow)
        {
            var title = new StringBuilder(User32.GetWindowTextLength(handleWindow) + 1);
            _ = User32.GetWindowText(handleWindow, title, title.Capacity);

            var tid = User32.GetWindowThreadProcessId(handleWindow, out uint pid);

            var pHandle = Kernel32.OpenProcess(0x0400, false, pid);
            if (pHandle == null)
            {
                throw new Exception($"cannot load thread info {Kernel32.GetLastError()}");
            }
            var filepath = new StringBuilder(Kernel32.MAX_PATH + 1);
            uint size = Kernel32.MAX_PATH + 1;
            var result = Kernel32.QueryFullProcessImageName(pHandle, 0, filepath, ref size);

            if (!result)
            {
                throw new Exception($"cannot get path info {Kernel32.GetLastError()}");
            }

            Mode = mode;
            WindowName = title.ToString();
            Tid = tid;
            Pid = pid;
            Executable = filepath.ToString();
        }

        public bool needFliterOut(ISet<String> set)
        {
            return set.Contains(Executable);
        }

        public override int GetHashCode()
        {
            return Mode switch
            {
                ObserveMode.Executable => Executable.GetHashCode(),
                ObserveMode.Process => Pid.GetHashCode(),
                ObserveMode.Thread => Tid.GetHashCode() + Pid.GetHashCode(),
                ObserveMode.TitleName => WindowName.GetHashCode() + Pid.GetHashCode(),
                _ => base.GetHashCode(),
            };
        }

        public bool Equals(WindowsInfo? other)
        {
            if (other == null)
            {
                return false;
            }
            return Mode switch
            {
                ObserveMode.Executable => Executable.Equals(other.Executable),
                ObserveMode.Process => Pid.Equals(other.Pid),
                ObserveMode.Thread => Tid.Equals(other.Tid) && Pid.Equals(other.Pid),
                ObserveMode.TitleName => WindowName.Equals(other.WindowName) && Pid.Equals(other.Pid),
                _ => false,
            };
        }

        public override bool Equals(object obj) => Equals(obj as WindowsInfo);
    }
}
