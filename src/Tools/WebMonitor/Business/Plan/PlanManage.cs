using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Agebull.Common.Configuration;
using Agebull.Common.Logging;
using Agebull.Common.Tson;
using Agebull.ZeroNet.Core;
using Agebull.ZeroNet.PubSub;
using Agebull.ZeroNet.ZeroApi;
using Newtonsoft.Json;
using WebMonitor;
using ZeroMQ;

namespace ZeroNet.Http.Route
{

    /// <summary>
    /// 计划管理器
    /// </summary>
    public class PlanManage : ZeroManageCommand
    {
        #region 实例
        /// <summary>
        /// 地址错误的情况
        /// </summary>
        /// <returns></returns>
        protected sealed override string GetAddress()
        {
            var config = ZeroApplication.Config["PlanDispatcher"];
            if (config == null)
                return null;
            return config.RequestAddress;
        }


        /// <summary>
        /// 构造路由计数器
        /// </summary>
        public PlanManage()
        {
            ManageAddress = GetAddress();
            FlushList();
        }

        #endregion

        public static Dictionary<long, ZeroPlan> Plans = new Dictionary<long, ZeroPlan>();

        public static void OnPlanEvent(ZeroNetEventType eventType, ZeroPlan plan)
        {
            switch (eventType)
            {
                case ZeroNetEventType.PlanAdd:
                    SyncPlan(plan);
                    break;
                case ZeroNetEventType.PlanRemove:
                    RemovePlan(plan);
                    break;
                default:
                    UpdatePlan(plan);
                    break;
            }
        }
        public static void RemovePlan(ZeroPlan plan)
        {
            if (Plans.TryGetValue(plan.plan_id, out var old))
            {
                Plans.Remove(plan.plan_id);
                plan.plan_state = plan_message_state.remove;
                WebSocketNotify.Publish("plan_notify", "remove", JsonConvert.SerializeObject(plan));
            }
        }
        public static void UpdatePlan(ZeroPlan plan)
        {
            if (Plans.TryGetValue(plan.plan_id, out var old))
            {
                old.exec_time = plan.exec_time;
                old.exec_state = plan.exec_state;
                old.plan_state = plan.plan_state;
                old.plan_time = plan.plan_time;
                old.real_repet = plan.real_repet;
                old.skip_set = plan.skip_set;
                old.skip_num = plan.skip_num;
                WebSocketNotify.Publish("plan_notify", "update", JsonConvert.SerializeObject(plan));
            }
        }

        public static void SyncPlan(ZeroPlan plan)
        {
            if (Plans.ContainsKey(plan.plan_id))
            {
                Plans[plan.plan_id] = plan;
            }
            else
            {
                Plans.Add(plan.plan_id, plan);
            }

            WebSocketNotify.Publish("plan_notify", "add", JsonConvert.SerializeObject(plan));
        }


        public ApiResult Pause(string id)
        {
            if (!long.TryParse(id, out var pid) || !Plans.TryGetValue(pid, out var plan))
                return ApiResult.Error(ErrorCode.NetworkError, "参数错误");
            var result = CallCommand("pause", $"msg:{plan.station}:{plan.plan_id:x}");
            if (result.State != ZeroOperatorStateType.Ok)
            {
                return ApiResult.Error(ErrorCode.NetworkError, "参数错误");
            }
            return ApiResult.Succees();
        }

        public ApiResult Reset(string id)
        {
            if (!long.TryParse(id, out var pid) || !Plans.TryGetValue(pid, out var plan))
                return ApiResult.Error(ErrorCode.NetworkError, "参数错误");
            var result = CallCommand("reset", $"msg:{plan.station}:{plan.plan_id:x}");
            if (result.State != ZeroOperatorStateType.Ok)
            {
                return ApiResult.Error(ErrorCode.NetworkError, "参数错误");
            }
            return ApiResult.Succees();
        }



        public ApiResult Remove(string id)
        {
            if (!long.TryParse(id, out var pid) || !Plans.TryGetValue(pid, out var plan))
                return ApiResult.Error(ErrorCode.NetworkError, "参数错误");
            var result = CallCommand("remove", $"msg:{plan.station}:{plan.plan_id:x}");
            if (result.State != ZeroOperatorStateType.Ok)
            {
                return ApiResult.Error(ErrorCode.NetworkError, "参数错误");
            }
            return ApiResult.Succees();
        }


        public ApiResult Close(string id)
        {
            if (!long.TryParse(id, out var pid) || !Plans.TryGetValue(pid, out var plan))
                return ApiResult.Error(ErrorCode.NetworkError, "参数错误");
            var result = CallCommand("close", $"msg:{plan.station}:{plan.plan_id:x}");
            if (result.State != ZeroOperatorStateType.Ok)
            {
                return ApiResult.Error(ErrorCode.NetworkError, "参数错误");
            }
            return ApiResult.Succees();
        }
        public IApiResult History()
        {
            return new ApiArrayResult<ZeroPlan>
            {
                ResultData = Plans.Values.Where(p => p.plan_state >= plan_message_state.close).ToArray()
            };
        }
        public IApiResult Station(string station)
        {
            if (string.IsNullOrEmpty(station))
            {
                return ApiResult.Error(ErrorCode.NetworkError, "参数错误");
            }
            station = station.Split('-').LastOrDefault();
            return new ApiArrayResult<ZeroPlan>
            {
                ResultData = Plans.Values.Where(p => p.station.Equals(station, StringComparison.OrdinalIgnoreCase)).ToArray()
            };
        }

        public IApiResult Active()
        {
            return new ApiArrayResult<ZeroPlan>
            {
                ResultData = Plans.Values.Where(p => p.plan_state < plan_message_state.close).ToArray()
            };
        }
        public IApiResult Filter(string type)
        {
            if (type == "delay")
            {
                return new ApiArrayResult<ZeroPlan>
                {
                    ResultData = Plans.Values.Where(p => p.plan_type >= plan_date_type.second && p.plan_type <= plan_date_type.day).ToArray()
                };
            }
            if (!Enum.TryParse<plan_date_type>(type, out var planType))
                return ApiResult.Error(ErrorCode.NetworkError, "参数错误");
            return new ApiArrayResult<ZeroPlan>
            {
                ResultData = Plans.Values.Where(p => p.plan_type == planType).ToArray()
            };
        }
        public ApiResult FlushList()
        {
            var result = CallCommand("list");
            if (result.State != ZeroOperatorStateType.Ok)
            {
                return ApiResult.Error(ErrorCode.NetworkError, "参数错误");
            }
            if (!result.TryGetValue(ZeroFrameType.Status, out var json))
            {
                ZeroTrace.WriteError("FlushList", "Empty");
                return ApiResult.Error(ErrorCode.LogicalError, "空数据");
            }

            try
            {
                var list = JsonConvert.DeserializeObject<List<ZeroPlan>>(json);
                foreach (var plan in list)
                {
                    SyncPlan(plan);
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
        private readonly byte[] planApiDescription =
        {
            4,
            (byte)ZeroByteCommand.Plan,
            ZeroFrameType.Plan,
            ZeroFrameType.Context,
            ZeroFrameType.Command,
            ZeroFrameType.Argument,
            ZeroFrameType.End
        };

        /// <summary>
        ///     请求格式说明
        /// </summary>
        private readonly byte[] planPubDescription =
        {
            4,
           (byte) ZeroByteCommand.Plan,
            ZeroFrameType.Plan,
            ZeroFrameType.Context,
            ZeroFrameType.PubTitle,
            ZeroFrameType.Content,
            ZeroFrameType.End
        };

        /// <summary>
        ///     命令格式说明
        /// </summary>
        private readonly byte[] commandDescription =
        {
            3,
           (byte) ZeroByteCommand.Plan,
            ZeroFrameType.Plan,
            ZeroFrameType.Command,
            ZeroFrameType.Argument,
            ZeroFrameType.End
        };

        public ApiResult NewPlan(ClientPlan clientPlan)
        {
            if (string.IsNullOrWhiteSpace(clientPlan.station) || string.IsNullOrWhiteSpace(clientPlan.command))
                return ApiResult.Error(ErrorCode.LogicalError, "命令不能为空");
            var config = ZeroApplication.Config[clientPlan.station];
            if (config == null)
                return ApiResult.Error(ErrorCode.LogicalError, "站点名称错误");
            if (config.StationType != ZeroStationType.Dispatcher && config.IsBaseStation)
                return ApiResult.Error(ErrorCode.LogicalError, "不允许对基础站点设置计划(SystemManage除外)");

            clientPlan.station = config.StationName;


            CustomPlan plan = new CustomPlan
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
            var socket = ZeroConnectionPool.GetSocket(clientPlan.station, null);
            if (socket.Socket == null)
                return ApiResult.Error(ErrorCode.LocalError, "无法联系ZeroCenter");
            bool success;

            if (config.StationType == ZeroStationType.Api || config.StationType == ZeroStationType.Vote)
            {
                success = socket.Socket.SendTo(planApiDescription,
                    plan.ToZeroBytes(),
                    clientPlan.context.ToZeroBytes(),
                    clientPlan.command.ToZeroBytes(),
                    clientPlan.argument.ToZeroBytes());
            }
            else if (config.StationType == ZeroStationType.Publish)
            {
                success = socket.Socket.SendTo(planPubDescription,
                    plan.ToZeroBytes(),
                    clientPlan.context.ToZeroBytes(),
                    clientPlan.command.ToZeroBytes(),
                    clientPlan.argument.ToZeroBytes());
            }
            else//Manage
            {
                clientPlan.command = clientPlan.command.ToLower();
                if (clientPlan.command != "pause" && clientPlan.command != "close" && clientPlan.command != "resume")
                    return ApiResult.Error(ErrorCode.LogicalError, "系统命令仅支持暂停(pause)关闭(close)和恢复(resume) 非系统站点");
                config = ZeroApplication.Config[clientPlan.station];
                if (config == null || config.State == ZeroCenterState.Stop || config.IsBaseStation)
                    return ApiResult.Error(ErrorCode.LogicalError, "命令参数为有效的非系统站点名称");

                success = socket.Socket.SendTo(commandDescription,
                    plan.ToZeroBytes(),
                    clientPlan.command.ToZeroBytes(),
                    clientPlan.argument.ToZeroBytes());
            }
            if (!success)
            {
                ZeroTrace.WriteInfo("NewPlan", "Send", socket.Socket.GetLastError());

                return ApiResult.Error(ErrorCode.NetworkError, socket.Socket.GetLastError().Text);
            }
            if (!socket.Socket.Recv(out var message))
            {
                ZeroTrace.WriteInfo("NewPlan", "Recv", socket.Socket.LastError);
                return ApiResult.Error(ErrorCode.NetworkError, socket.Socket.GetLastError().Text);
            }
            var value = message.Unpack();

            return value.State == ZeroOperatorStateType.Plan ? ApiResult.Succees() : ApiResult.Error(ErrorCode.LogicalError, value.State.Text());
        }
    }
}