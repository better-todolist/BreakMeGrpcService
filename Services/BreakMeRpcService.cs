using Grpc.Core;
using BreakMe;
using BreakMeGrpcService.DataObj;
using BreakMeGrpcService.Local;


namespace BreakMeGrpcService.Services
{
    public class BreakMeRpcService : BreakMe.BreakMe.BreakMeBase
    {
        private readonly ILogger<BreakMeRpcService> _logger;
        public BreakMeRpcService(ILogger<BreakMeRpcService> logger)
        {
            _logger = logger;
        }

        public override async Task<CreateResp> CreateInterrupt(Interrupt request, ServerCallContext context)
        {
            _logger.Log(LogLevel.Information, $"Create Intrupt");
            InterruptData data = new(
                Guid.NewGuid(),
                request.Time,
                request.HasProgressPath ? request.ProgressPath : null,
                request.Ty,
                request.ObserveMode,
                request.Message);

            var uid = await FileManager.CreateIntp(data);

            if (uid == null)
            {
                return new CreateResp { IsSuccess = false, Guid = "" };

            }
            else
            {
                return new CreateResp { IsSuccess = true, Guid = uid.ToString() };
            }
        }



        public override async Task<InterruptList> FetchAllInterrupt(BreakMe.Empty request, ServerCallContext context)
        {
            var ret_list = new InterruptList();
            foreach (var item in await FileManager.GetAllIntpTask())
            {
                var tmp = new Interrupt
                {

                    ProgressPath = item.TargetProgress ?? "",
                    Time = item.IntpTime,
                    Ty = item.InterruptType,
                    ObserveMode = item.ObserveMode,
                    Message = item.IntpMessage,
                };
                ret_list.AllInfo.Add(item.Id.ToString(), tmp);
            }
            return ret_list;
        }

        public override async Task<OperateResp> UpdateInterrupt(EditedInterrupt request, ServerCallContext context)
        {
            InterruptData data = new(
                new Guid(request.Guid), request.New.Time, request.New.ProgressPath, request.New.Ty, request.New.ObserveMode, request.New.ProgressPath);

            _ = await FileManager.CreateIntp(data);

            return new OperateResp { IsSuccess = true };
        }

        public override Task<OperateResp> RemoveInterrupt(InterruptUid request, ServerCallContext context)
        {
            FileManager.RemoveIntp(new Guid(request.Guid));
            return Task.FromResult(new OperateResp { IsSuccess = true });
        }

        public override async Task<OperateResp> StartInterrupt(InterruptUid request, ServerCallContext context)
        {
            var _data = await FileManager.GetIntpInfo(new Guid(request.Guid));
            // TODO: start the monitor

            return new OperateResp { IsSuccess = false };
        }


        public override async Task<Config> FetchConfig(BreakMe.Empty request, ServerCallContext context)
        {
            _logger.Log(LogLevel.Information, $"Fetch Config");
            var cfg = await FileManager.GetConfig();
            var ret_cfg = new Config { LeaveTimeBound = cfg.LeaveTimeBound, MultiTaskNum = cfg.MuiltTaskNum };

            ret_cfg.WhiteList.AddRange(cfg.WhiteList);

            return ret_cfg;
        }

        public override async Task<OperateResp> UpdateConfig(SetConfigReq request, ServerCallContext context)
        {
            var old = await FileManager.GetConfig();

            if (request.HasLeaveTimeBound)
            {
                old.LeaveTimeBound = request.LeaveTimeBound;
            }
            if (request.HasMultiTaskNum)
            {
                old.MuiltTaskNum = request.MultiTaskNum;
            }

            var wihtelist = request.WhiteList;
            if (wihtelist.Mode == WhiteListUpdateMode.Overwrite)
            {
                old.WhiteList = wihtelist.PrgressPathList;
            }
            else
            {

                foreach (var item in wihtelist.PrgressPathList)
                {
                    old.WhiteList.Add(item);
                }

            }

            FileManager.updateConfig(old);

            return new OperateResp { IsSuccess = true };
        }
    }


}