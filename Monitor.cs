using BreakMe;
using BreakMeGrpcService.DataObj;
using BreakMeGrpcService.Local;
using Microsoft.Toolkit.Uwp.Notifications;
using System.Text;
using Vanara.PInvoke;

namespace BreakMeGrpcService
{

    public class Monitor
    {

        public Monitor()
        {

        }

        private WindowsInfo getNowWindows(ObserveMode mode)
        {
            var windows = User32.GetForegroundWindow();
            return new WindowsInfo(mode, windows);
        }

        public async void StartObserve(InterruptData intp)
        {
            var cfg = await FileManager.GetConfig();

            Task task = Task.Run(async () =>
            {
                await Observe(intp, cfg);
            });
        }

        public Task Observe(InterruptData data, LocalConfig cfg)
        {
            const int detectionInterval = 500;
            const int monitorInterval = 5000;


            const int confirmTime = 30 * 60 * 1000 / detectionInterval;
            var monitorTime = data.IntpTime * 30 * 60 * 1000 / monitorInterval;
            var leaveTime = cfg.LeaveTimeBound;

            var taskCounter = new Dictionary<WindowsInfo, int>(cfg.MuiltTaskNum);
            var counter = 0;
            var leaveCounter = 0;
            var monitorMode = false;

            HashSet<WindowsInfo> remainder = new();

            var timer = new System.Timers.Timer(detectionInterval)
            {
                AutoReset = true,
                Enabled = true
            };

            timer.Elapsed += (sender, e) =>
            {
                var info = getNowWindows(data.ObserveMode);

                // 白名单应用，去除
                if (info.needFliterOut(new HashSet<string>(cfg.WhiteList)))
                {
                    return;
                }
                // 如果指定了监控目标进程且当前进程与目标监控进程不同，去除
                if(data.TargetProgress != null && data.TargetProgress != info.Executable) {
                    return;
                }

                // 第一阶段，确认监控进程
                if (!monitorMode)
                {
                    // 对应进程计数器加1
                    if (taskCounter.TryGetValue(info, out var value))
                    {
                        taskCounter[info] = value + 1;
                    }
                    else
                    {
                        taskCounter[info] = 1;
                    }
                    counter++;
                    if (counter >= confirmTime)
                    {
                        monitorMode = true;
                        // 根据多任务数量，确定监控进程
                        remainder = taskCounter.OrderBy((k) => k.Value).Take(cfg.MuiltTaskNum).Select((v) => v.Key).ToHashSet();
                        timer.Interval = monitorInterval;
                    }
                }
                else
                {
                    // 第二阶段，开始监控到结束

                    // 监控到进入了非专注进程
                    if (!remainder.Contains(info))
                    {
                        leaveCounter += monitorInterval;
                        return;
                    }
                    // 已经离开
                    if (leaveCounter >= leaveTime)
                    {
                        timer.Stop();
                        timer.Close();
                    }
                    // 时间到
                    if(counter >= monitorTime)
                    {
                        switch (data.InterruptType) {
                            case InterruptType.Notify:
                                // 发送 toast Notify
                                new ToastContentBuilder()
                                .AddText("Break Me!")
                                .AddText(data.IntpMessage)
                                .Show();

                                break;
                            default:
                                // TODO 启动打断窗口
                                break;
                        }

                    }




                }


            };

            timer.Start();
            return Task.CompletedTask;
        }
    }


    public sealed class WindowsInfo : IEquatable<WindowsInfo>
    {
        private readonly ObserveMode Mode;
        public String WindowName { get; set; }
        public uint Tid { get; set; }
        public uint Pid { get; set; }
        public String Executable { get; set; }

        public POINT CursorPos { get; set; }

        public bool IsScreenSaverRunning { get; set; }

        public WindowsInfo(ObserveMode mode, HWND handleWindow)
        {
            var title = new StringBuilder(User32.GetWindowTextLength(handleWindow) + 1);
            _ = User32.GetWindowText(handleWindow, title, title.Capacity);

            var tid = User32.GetWindowThreadProcessId(handleWindow, out uint pid);

            var pHandle = Kernel32.OpenProcess(0x0400, false, pid) ?? throw new Exception($"cannot load thread info {Kernel32.GetLastError()}");

            var filepath = new StringBuilder(Kernel32.MAX_PATH + 1);
            uint size = Kernel32.MAX_PATH + 1;
            var result = Kernel32.QueryFullProcessImageName(pHandle, 0, filepath, ref size);

            if (!result)
            {
                throw new Exception($"cannot get path info {Kernel32.GetLastError()}");
            }

            User32.GetCursorPos(out POINT CursorPos);

            Mode = mode;
            WindowName = title.ToString();
            Tid = tid;
            Pid = pid;
            Executable = filepath.ToString();
            this.CursorPos = CursorPos;
            IsScreenSaverRunning = false;
        }

        public bool needFliterOut(ISet<string> set)
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
