using System.Text.Json.Serialization;

namespace BreakMeGrpcService.DataObj
{
    public sealed class LocalConfig
    {
        public IList<string> WhiteList  { get; set; }
        public long LeaveTimeBound { get; set; }
        public int MuiltTaskNum { get; set; }



        [JsonConstructor] public LocalConfig() { }
         LocalConfig(IList<string> whiteList, long leaveTimeBound, int muiltTaskNum)
        {
            this.WhiteList = whiteList ;
            LeaveTimeBound = leaveTimeBound;
            MuiltTaskNum = muiltTaskNum;
        }

        public static LocalConfig get_default()
        {
            return new LocalConfig(new List<string>(),1000 * 60 *3,1);
        }
    }
}
