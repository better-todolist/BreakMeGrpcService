using BreakMe;
using System.Text.Json.Serialization;

namespace BreakMeGrpcService.DataObj
{
    public sealed class InterruptData
    {
        public Guid Id { get; set; }
        public int IntpTime { get; set; }
        public string? TargetProgress { get; set; }
        public InterruptType InterruptType { get; set; }
        public ObserveMode ObserveMode { get; set; }
        public string? IntpMessage { get; set; }

        [JsonConstructor] public InterruptData() { }

        public InterruptData(Guid id, int intpTime, string? targetProgress=null, InterruptType interruptType = InterruptType.Notify, ObserveMode observeMode = ObserveMode.Executable, string? intpMessage = null)
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
