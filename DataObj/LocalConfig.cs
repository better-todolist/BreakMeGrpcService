namespace BreakMeGrpcService.DataObj
{
    public sealed class LocalConfig
    {
        public IList<string> WhiteList  { get; set; }
        public Int64 LeaveTimeBound { get; set; }
        public Int32 MuiltTaskNum { get; set; }

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
