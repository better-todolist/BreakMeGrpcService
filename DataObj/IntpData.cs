using BreakMe;

namespace BreakMeGrpcService.DataObj
{
    public sealed class IntpData
    {
        public Guid Id { get; set; }
        public Int32 IntpTime { get; set; }
        public String? TargetProgress { get; set; }
        public IntpType InterruptType { get; set; }
        public ObserveMode ObserveMode { get; set; }
        public string? IntpMessage { get; set; }

        public IntpData(Guid id, int intpTime, String? targetProgress=null, IntpType interruptType = IntpType.Notify, ObserveMode observeMode = ObserveMode.Executable, string? intpMessage = null)
        {
            Id = id;
            IntpTime = intpTime;
            this.TargetProgress = targetProgress;
            this.InterruptType = interruptType;
            this.ObserveMode = observeMode;
            this.IntpMessage = intpMessage;
        }
    }

}
