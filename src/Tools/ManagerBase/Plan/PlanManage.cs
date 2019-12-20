using System;
using System.Collections.Generic;
using System.Linq;

using Agebull.MicroZero;
using Agebull.EntityModel.Common;
using Newtonsoft.Json;
using WebMonitor;
using Agebull.MicroZero.ZeroApis;
using System.Text;
using System.Threading.Tasks;
using ZeroMQ;

namespace MicroZero.Http.Route
{

    /// <summary>
    /// 计划管理器
    /// </summary>
    public class PlanManage : ZSimpleCommand
    {
        #region 实例
        
        /// <summary>
        /// 构造路由计数器
        /// </summary>
        public PlanManage()
        {
            ManageAddress= ZeroApplication.Config["PlanDispatcher"]?.RequestAddress;
            FlushList().Wait();
        }

        #endregion

        public static Dictionary<string, ZeroPlan> Plans = new Dictionary<string, ZeroPlan>();

        public static async Task OnPlanEvent(ZeroNetEventType eventType, ZeroPlan plan)
        {
            switch (eventType)
            {
                case ZeroNetEventType.PlanAdd:
                    await SyncPlan(plan);
                    break;
                case ZeroNetEventType.PlanRemove:
                    await RemovePlan(plan);
                    break;
                default:
                   await UpdatePlan(plan);
                    break;
            }
        }
        public static async Task RemovePlan(ZeroPlan plan)
        {
            if (!Plans.TryGetValue(plan.name, out _)) return;
            Plans.Remove(plan.name);
            plan.plan_state = plan_message_state.remove;
            await WebSocketNotify.Publish("plan_notify", "remove", JsonHelper.SerializeObject(plan));
        }
        public static async Task UpdatePlan(ZeroPlan plan)
        {
            if (!Plans.TryGetValue(plan.name, out var old)) return;
            old.exec_time = plan.exec_time;
            old.exec_state = plan.exec_state;
            old.plan_state = plan.plan_state;
            old.plan_time = plan.plan_time;
            old.real_repet = plan.real_repet;
            old.skip_set = plan.skip_set;
            old.skip_num = plan.skip_num;
            await WebSocketNotify.Publish("plan_notify", "update", JsonHelper.SerializeObject(plan));
        }

        public static async Task SyncPlan(ZeroPlan plan)
        {
            if (Plans.ContainsKey(plan.name))
            {
                Plans[plan.name] = plan;
            }
            else
            {
                Plans.Add(plan.name, plan);
            }

            await WebSocketNotify.Publish("plan_notify", "add", JsonHelper.SerializeObject(plan));
        }


        public async Task<ApiResult> Pause(string id)
        {
            if (!Plans.ContainsKey(id))
                return ApiResult.Error(ErrorCode.LogicalError, "参数错误");
            var result = await CallCommand("pause", id);
            return result.State != ZeroOperatorStateType.Ok ? ApiResult.Error(ErrorCode.LogicalError, "参数错误") : ApiResult.Succees();
        }

        public async Task<ApiResult> Reset(string id)
        {
            if (!Plans.ContainsKey(id))
                return ApiResult.Error(ErrorCode.LogicalError, "参数错误");
            var result = await CallCommand("reset", id);
            return result.State != ZeroOperatorStateType.Ok ? ApiResult.Error(ErrorCode.LogicalError, "参数错误") : ApiResult.Succees();
        }


        public async Task<ApiResult> Clear()
        {
            StringBuilder keys = new StringBuilder();
            foreach (var plan in Plans.Where(p => p.Value.plan_state == plan_message_state.close).ToArray())
            {
                var result = await CallCommand("remove", plan.Value.name);
                if (result.State != ZeroOperatorStateType.Ok)
                    continue;
                Plans.Remove(plan.Key);
                keys.AppendLine(plan.Value.name);
            }
            return ApiResult.Succees(keys.ToString());
        }


        public ApiResult Test()
        {
            return ApiResult.Succees(Plans.Values.ToList());
        }

        public async Task<ApiResult> Remove(string id)
        {
            if (!Plans.ContainsKey(id))
                return ApiResult.Error(ErrorCode.LogicalError, "参数错误");
            var result = await CallCommand("remove", id);
            return result.State != ZeroOperatorStateType.Ok ? ApiResult.Error(ErrorCode.LogicalError, "参数错误") : ApiResult.Succees();
        }


        public async Task<ApiResult> Close(string id)
        {
            if (!Plans.ContainsKey(id))
                return ApiResult.Error(ErrorCode.LogicalError, "参数错误");
            var result = await CallCommand("close", id);
            return result.State != ZeroOperatorStateType.Ok ? ApiResult.Error(ErrorCode.LogicalError, "参数错误") : ApiResult.Succees();
        }
        public IApiResult History()
        {
            return new ApiArrayResult<ZeroPlan>
            {
                ResultData = Plans.Values.Where(p => p.plan_state >= plan_message_state.close).ToList()
            };
        }
        public IApiResult Station(string station)
        {
            if (string.IsNullOrEmpty(station))
            {
                return ApiResult.Error(ErrorCode.LogicalError, "参数错误");
            }
            station = station.Split('-').LastOrDefault();
            return new ApiArrayResult<ZeroPlan>
            {
                ResultData = Plans.Values.Where(p => p.station.Equals(station, StringComparison.OrdinalIgnoreCase)).ToList()
            };
        }

        public IApiResult Active()
        {
            return new ApiArrayResult<ZeroPlan>
            {
                ResultData = Plans.Values.Where(p => p.plan_state < plan_message_state.close).ToList()
            };
        }
        public IApiResult Filter(string type)
        {
            if (type == "delay")
            {
                return new ApiArrayResult<ZeroPlan>
                {
                    ResultData = Plans.Values.Where(p => p.plan_type >= plan_date_type.second && p.plan_type <= plan_date_type.day).ToList()
                };
            }
            if (!Enum.TryParse<plan_date_type>(type, out var planType))
                return ApiResult.Error(ErrorCode.LogicalError, "参数错误");
            return new ApiArrayResult<ZeroPlan>
            {
                ResultData = Plans.Values.Where(p => p.plan_type == planType).ToList()
            };
        }
        public async Task<ApiResult> FlushList()
        {
            var result = await CallCommand("list");
            if (result.State != ZeroOperatorStateType.Ok)
            {
                return ApiResult.Error(ErrorCode.LogicalError, "参数错误");
            }
            if (!result.TryGetString(ZeroFrameType.Status, out var json))
            {
                ZeroTrace.WriteError("FlushList", "Empty");
                return ApiResult.Error(ErrorCode.LogicalError, "空数据");
            }

            try
            {
                var list = JsonConvert.DeserializeObject<List<ZeroPlan>>(json);
                Plans.Clear();
                foreach (var plan in list)
                {
                    await SyncPlan(plan);
                }
                return ApiResult.Succees();
            }
            catch (Exception ex)
            {
                ZeroTrace.WriteException("FlushList", ex, json);
                return ApiResult.Error(ErrorCode.LocalException, "内部服务");
            }

        }



        /// <summary>
        ///     请求格式说明
        /// </summary>
        private readonly byte[] _planApiDescription =
        {
            5,
            (byte)ZeroByteCommand.Plan,
            ZeroFrameType.Plan,
            ZeroFrameType.Context,
            ZeroFrameType.Command,
            ZeroFrameType.Argument,
            ZeroFrameType.SerivceKey,
            ZeroFrameType.End
        };

        /// <summary>
        ///     请求格式说明
        /// </summary>
        private readonly byte[] _planPubDescription =
        {
            5,
           (byte) ZeroByteCommand.Plan,
            ZeroFrameType.Plan,
            ZeroFrameType.Context,
            ZeroFrameType.PubTitle,
            ZeroFrameType.TextContent,
            ZeroFrameType.SerivceKey,
            ZeroFrameType.End
        };

        /// <summary>
        ///     命令格式说明
        /// </summary>
        private readonly byte[] commandDescription =
        {
            4,
           (byte) ZeroByteCommand.Plan,
            ZeroFrameType.Plan,
            ZeroFrameType.Command,
            ZeroFrameType.Argument,
            ZeroFrameType.SerivceKey,
            ZeroFrameType.End
        };

        public async Task<ApiResult> NewPlan(ClientPlan clientPlan)
        {
            if (string.IsNullOrWhiteSpace(clientPlan.station) || string.IsNullOrWhiteSpace(clientPlan.command))
                return ApiResult.Error(ErrorCode.LogicalError, "命令不能为空");
            var config = ZeroApplication.Config[clientPlan.station];
            if (config == null)
                return ApiResult.Error(ErrorCode.LogicalError, "站点名称错误");
            if (config.IsSystem)
                return ApiResult.Error(ErrorCode.LogicalError, "不允许对基础站点设置计划");

            clientPlan.station = config.StationName;


            var plan = new ZeroPlanInfo
            {
                plan_type = clientPlan.plan_type,
                plan_value = clientPlan.plan_value,
                plan_repet = clientPlan.plan_repet,
                description = clientPlan.description,
                no_skip = clientPlan.no_skip,
                plan_time = clientPlan.plan_time < new DateTime(1970, 1, 1) ? 0 : (int)((clientPlan.plan_time.ToUniversalTime().Ticks - 621355968000000000) / 10000000),
                skip_set = clientPlan.skip_set
            };
            if (clientPlan.plan_type == plan_date_type.week && plan.plan_value == 7)
            {
                plan.plan_value = 0;
            }
            if (clientPlan.plan_type > plan_date_type.time)
            {
                if (clientPlan.skip_set > 0)
                    plan.plan_repet += clientPlan.skip_set;
            }
            else
            {
                plan.skip_set = 0;
                plan.plan_repet = 1;
            }

            var socket = ApiProxy.GetSocket(clientPlan.station, null);
            if (socket == null)
                return ApiResult.Error(ErrorCode.LocalError, "无法联系ZeroCenter");

            using (socket)
            {
                bool success;
                ZMessage message;
                switch (config.StationType)
                {
                    case ZeroStationType.Api:
                    case ZeroStationType.Vote:
                        message = new ZMessage(clientPlan.station.ToZeroBytes(),
                            _planApiDescription,
                            plan.ToZeroBytes(),
                            clientPlan.context.ToZeroBytes(),
                            clientPlan.command.ToZeroBytes(),
                            clientPlan.argument.ToZeroBytes(),
                            ServiceKey);
                        break;
                    //Manage
                    case ZeroStationType.Notify:
                        message = new ZMessage(clientPlan.station.ToZeroBytes(),
                            _planPubDescription,
                            plan.ToZeroBytes(),
                            clientPlan.context.ToZeroBytes(),
                            clientPlan.command.ToZeroBytes(),
                            clientPlan.argument.ToZeroBytes(),
                            ServiceKey);
                        break;
                    default:
                        clientPlan.command = clientPlan.command.ToLower();
                        if (clientPlan.command != "pause" && clientPlan.command != "close" && clientPlan.command != "resume")
                            return ApiResult.Error(ErrorCode.LogicalError, "系统命令仅支持暂停(pause)关闭(close)和恢复(resume) 非系统站点");
                        config = ZeroApplication.Config[clientPlan.station];
                        if (config == null)
                            return ApiResult.Error(ErrorCode.LogicalError, "站点名称无效");
                        if (config.IsSystem)
                            return ApiResult.Error(ErrorCode.LogicalError, "不允许对内置站点设置计划");
                        message = new ZMessage(clientPlan.station.ToZeroBytes(),
                            commandDescription,
                            plan.ToZeroBytes(),
                            clientPlan.command.ToZeroBytes(),
                            clientPlan.argument.ToZeroBytes(),
                            ServiceKey);
                        break;
                }
                using (message)
                    success = await socket.SendToAsync(message);
                if (!success)
                {
                    ZeroTrace.SystemLog("NewPlan", "Send", socket.GetLastError());

                    return ApiResult.Error(ErrorCode.NetworkError, socket.GetLastError().Text);
                }

                message = await socket.RecvAsync();
                if (message == null)
                {
                    ZeroTrace.SystemLog("NewPlan", "Recv", socket.LastError);
                    return ApiResult.Error(ErrorCode.NetworkError, socket.GetLastError().Text);
                }

                PlanItem.UnpackResult(message, out var item);
                return item.State == ZeroOperatorStateType.Plan ? ApiResult.Succees() : ApiResult.Error(ErrorCode.LogicalError, item.State.Text());

            }

        }
    }
}