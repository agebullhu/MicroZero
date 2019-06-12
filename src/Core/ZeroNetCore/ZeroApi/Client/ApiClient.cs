using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Agebull.Common.Logging;

namespace Agebull.MicroZero.ZeroApis
{
    /// <summary>
    ///     Api站点
    /// </summary>
    public class ApiClient
    {
        #region Properties

        /// <summary>
        ///     返回的数据
        /// </summary>
        private readonly ApiClientCore _core = new ApiClientCore();

        /// <summary>
        ///     返回值
        /// </summary>
        public string Result => _core.Result;

        /// <summary>
        ///     请求站点
        /// </summary>
        public string Station { get => _core.Station; set => _core.Station = value; }

        /// <summary>
        ///     上下文内容（透传方式）
        /// </summary>
        public string ContextJson { get => _core.ContextJson; set => _core.ContextJson = value; }

        /// <summary>
        ///     标题
        /// </summary>
        public string Title { get => _core.Title; set => _core.Title = value; }

        /// <summary>
        ///     调用命令
        /// </summary>
        public string Commmand { get => _core.Commmand; set => _core.Commmand = value; }

        /// <summary>
        ///     参数
        /// </summary>
        public string Argument { get => _core.Argument; set => _core.Argument = value; }

        /// <summary>
        ///     扩展参数
        /// </summary>
        public string ExtendArgument { get => _core.ExtendArgument; set => _core.ExtendArgument = value; }

        /// <summary>
        ///     结果状态
        /// </summary>
        public ZeroOperatorStateType State => _core.State;

        /// <summary>
        /// 最后一个返回值
        /// </summary>
        public ZeroResult LastResult => _core.LastResult;

        /// <summary>
        /// 简单调用
        /// </summary>
        /// <remarks>
        /// 1 不获取全局标识(过时）
        /// 2 无远程定向路由,
        /// 3 无上下文信息
        /// </remarks>
        public bool Simple { set; get; }

        #endregion

        #region 流程


        /// <summary>
        ///     远程调用
        /// </summary>
        /// <returns></returns>
        public void CallCommand()
        {
            using (MonitorScope.CreateScope("内部Zero调用"))
            {
                Prepare();
                _core.Call();
                End();
            }
        }

        /// <summary>
        ///     检查在非成功状态下的返回值
        /// </summary>
        public void CheckStateResult()
        {
            _core.CheckStateResult();
        }


        #endregion

        #region async

        /// <summary>
        ///     远程调用
        /// </summary>
        /// <param name="station"></param>
        /// <param name="commmand"></param>
        /// <param name="argument"></param>
        /// <returns></returns>
        public static async Task<string> CallSync(string station, string commmand, string argument)
        {
            return await CallTask(station, commmand, argument);
        }

        /// <summary>
        ///     远程调用
        /// </summary>
        /// <param name="station"></param>
        /// <param name="commmand"></param>
        /// <param name="argument"></param>
        /// <returns></returns>
        public static Task<string> CallTask(string station, string commmand, string argument)
        {
            return Task.Factory.StartNew(() => Call(station, commmand, argument));
        }

        /// <summary>
        ///     远程调用
        /// </summary>
        /// <param name="station"></param>
        /// <param name="commmand"></param>
        /// <param name="argument"></param>
        /// <returns></returns>
        private static string Call(string station, string commmand, string argument)
        {
            var client = new ApiClient
            {
                Station = station,
                Commmand = commmand,
                Argument = argument
            };
            client.CallCommand();
            client._core.CheckStateResult();
            return client.Result;
        }


        #endregion

        #region 操作注入

        /// <summary>
        ///     Api处理器
        /// </summary>
        public interface IHandler
        {
            /// <summary>
            ///     准备
            /// </summary>
            /// <param name="item"></param>
            /// <returns></returns>
            void Prepare(ApiClient item);

            /// <summary>
            ///     结束处理
            /// </summary>
            /// <param name="item"></param>
            void End(ApiClient item);
        }

        /// <summary>
        ///     处理器
        /// </summary>
        private static readonly List<Func<IHandler>> ApiHandlers = new List<Func<IHandler>>();

        /// <summary>
        ///     注册处理器
        /// </summary>
        public static void RegistHandlers<THandler>() where THandler : class, IHandler, new()
        {
            ApiHandlers.Add(() => new THandler());
        }

        private readonly List<IHandler> _handlers = new List<IHandler>();

        /// <summary>
        ///     注册处理器
        /// </summary>
        public ApiClient()
        {
            foreach (var func in ApiHandlers)
                _handlers.Add(func());
        }


        private void Prepare()
        {
            if (Simple)
                return;
            LogRecorderX.MonitorTrace($"Station:{Station},Command:{Commmand}");
            foreach (var handler in _handlers)
                try
                {
                    handler.Prepare(this);
                }
                catch (Exception e)
                {
                    ZeroTrace.WriteException(Station, e, "PreActions", Commmand);
                }
        }

        private void End()
        {
            _core.CheckStateResult();
            if (Simple)
                return;
            LogRecorderX.MonitorTrace($"Result:{Result}");
            foreach (var handler in _handlers)
            {
                try
                {
                    handler.End(this);
                }
                catch (Exception e)
                {
                    ZeroTrace.WriteException(Station, e, "EndActions", Commmand);
                }
            }
        }

        #endregion

        #region 快捷方法

        /// <summary>
        /// 调用远程方法
        /// </summary>
        /// <typeparam name="TArgument">参数类型</typeparam>
        /// <typeparam name="TResult">返回值类型</typeparam>
        /// <param name="station">站点</param>
        /// <param name="api">api名称</param>
        /// <param name="arg">参数</param>
        /// <returns></returns>
        public static IApiResult<TResult> CallApi<TArgument, TResult>(string station, string api, TArgument arg)
        {
            var client = new ApiClient
            {
                Station = station,
                Commmand = api,
                Argument = arg == null ? null : JsonHelper.SerializeObject(arg)
            };
            client.CallCommand();
            if (client.State != ZeroOperatorStateType.Ok)
            {
                client._core.CheckStateResult();
            }
            return ApiResultIoc.Ioc.DeserializeObject<TResult>(client.Result);
        }
        /// <summary>
        /// 调用远程方法
        /// </summary>
        /// <typeparam name="TArgument">参数类型</typeparam>
        /// <param name="station">站点</param>
        /// <param name="api">api名称</param>
        /// <param name="arg">参数</param>
        /// <returns></returns>
        public static IApiResult CallApi<TArgument>(string station, string api, TArgument arg)
        {
            var client = new ApiClient
            {
                Station = station,
                Commmand = api,
                Argument = arg == null ? null : JsonHelper.SerializeObject(arg)
            };
            client.CallCommand();
            if (client.State != ZeroOperatorStateType.Ok)
            {
                client._core.CheckStateResult();
            }
            return ApiResultIoc.Ioc.DeserializeObject(client.Result);
        }

        /// <summary>
        /// 调用远程方法
        /// </summary>
        /// <typeparam name="TResult">参数类型</typeparam>
        /// <param name="station">站点</param>
        /// <param name="api">api名称</param>
        /// <returns></returns>
        public static IApiResult<TResult> CallApi<TResult>(string station, string api)
        {
            var client = new ApiClient
            {
                Station = station,
                Commmand = api
            };
            client.CallCommand();
            if (client.State != ZeroOperatorStateType.Ok)
            {
                client._core.CheckStateResult();
            }
            return ApiResultIoc.Ioc.DeserializeObject<TResult>(client.Result);
        }

        /// <summary>
        /// 调用远程方法
        /// </summary>
        /// <param name="station">站点</param>
        /// <param name="api">api名称</param>
        /// <returns></returns>
        public static IApiResult CallApi(string station, string api)
        {
            var client = new ApiClient
            {
                Station = station,
                Commmand = api
            };
            client.CallCommand();
            if (client.State != ZeroOperatorStateType.Ok)
            {
                client._core.CheckStateResult();
            }
            return ApiResultIoc.Ioc.DeserializeObject(client.Result);
        }

        /// <summary>
        /// 调用远程方法
        /// </summary>
        /// <typeparam name="TArgument">参数类型</typeparam>
        /// <typeparam name="TResult">返回值类型</typeparam>
        /// <param name="station">站点</param>
        /// <param name="api">api名称</param>
        /// <param name="arg">参数</param>
        /// <returns></returns>
        public static TResult Call<TArgument, TResult>(string station, string api, TArgument arg)
        {
            var client = new ApiClient
            {
                Station = station,
                Commmand = api,
                Argument = arg == null ? null : JsonHelper.SerializeObject(arg)
            };
            client.CallCommand();
            if (client.State != ZeroOperatorStateType.Ok)
            {
                client._core.CheckStateResult();
            }
            return JsonHelper.DeserializeObject<TResult>(client.Result);
        }

        /// <summary>
        /// 调用远程方法
        /// </summary>
        /// <typeparam name="TResult">参数类型</typeparam>
        /// <param name="station">站点</param>
        /// <param name="api">api名称</param>
        /// <returns></returns>
        public static TResult Call<TResult>(string station, string api)
        {
            var client = new ApiClient
            {
                Station = station,
                Commmand = api
            };
            client.CallCommand();
            if (client.State != ZeroOperatorStateType.Ok)
            {
                client._core.CheckStateResult();
            }
            return JsonHelper.DeserializeObject<TResult>(client.Result);
        }

        #endregion


        #region 计划调用(一次)

        /// <summary>
        /// 计划调用(一次)
        /// </summary>
        /// <typeparam name="TResult">返回值类型</typeparam>
        /// <param name="station">站点</param>
        /// <param name="api">api名称</param>
        /// <param name="time">计划时间</param>
        /// <param name="description">计划说明</param>
        /// <returns></returns>
        public static IApiResult<TResult> ApiPlan<TResult>(string station, string api, DateTime time, string description)
        {
            var client = new ApiClient
            {
                Station = station,
                Commmand = api
            };

            var plan = new ZeroPlanInfo
            {
                plan_type = plan_date_type.time,
                plan_value = 0,
                plan_repet = 1,
                description = description,
                no_skip = true,
                skip_set = 0,
                plan_time = (int)((time.ToUniversalTime().Ticks - 621355968000000000) / 10000000)
            };
            using (MonitorScope.CreateScope("内部Zero调用"))
            {
                client.Prepare();
                client._core.Plan(plan);
                client.End();
            }
            if (client.State != ZeroOperatorStateType.Ok)
            {
                client._core.CheckStateResult();
            }
            return ApiResultIoc.Ioc.DeserializeObject<TResult>(client.Result);
        }


        /// <summary>
        /// 计划调用(一次)
        /// </summary>
        /// <typeparam name="TResult">返回值类型</typeparam>
        /// <param name="station">站点</param>
        /// <param name="api">api名称</param>
        /// <param name="type">类型(除none time之外,基准时间为现在)</param>
        /// <param name="num">计划值</param>
        /// <param name="description">计划说明</param>
        /// <returns></returns>
        public static IApiResult<TResult> ApiPlan<TResult>(string station, string api,plan_date_type type, short num, string description)
        {
            var client = new ApiClient
            {
                Station = station,
                Commmand = api
            };

            var plan = new ZeroPlanInfo
            {
                plan_type = type,
                plan_value = num,
                plan_repet = 1,
                description = description,
                no_skip = true,
                skip_set = 0,
                plan_time = (int)((DateTime.Now.ToUniversalTime().Ticks - 621355968000000000) / 10000000)
            };
            using (MonitorScope.CreateScope("内部Zero调用"))
            {
                client.Prepare();
                client._core.Plan(plan);
                client.End();
            }
            if (client.State != ZeroOperatorStateType.Ok)
            {
                client._core.CheckStateResult();
            }
            return ApiResultIoc.Ioc.DeserializeObject<TResult>(client.Result);
        }

        /// <summary>
        /// 计划调用(一次)
        /// </summary>
        /// <typeparam name="TArgument">参数类型</typeparam>
        /// <typeparam name="TResult">返回值类型</typeparam>
        /// <param name="station">站点</param>
        /// <param name="api">api名称</param>
        /// <param name="arg">参数</param>
        /// <param name="time">计划时间</param>
        /// <param name="description">计划说明</param>
        /// <returns></returns>
        public static IApiResult<TResult> ApiPlan<TArgument, TResult>(string station, string api, TArgument arg,DateTime time,string description)
        {
            var client = new ApiClient
            {
                Station = station,
                Commmand = api,
                Argument = arg == null ? null : JsonHelper.SerializeObject(arg)
            };

            var plan = new ZeroPlanInfo
            {
                plan_type = plan_date_type.time,
                plan_value = 0,
                plan_repet = 1,
                description = description,
                no_skip = true,
                skip_set = 0,
                plan_time = (int)((time.ToUniversalTime().Ticks - 621355968000000000) / 10000000)
            };
            using (MonitorScope.CreateScope("内部Zero调用"))
            {
                client.Prepare();
                client._core.Plan(plan);
                client.End();
            }
            if (client.State != ZeroOperatorStateType.Ok)
            {
                client._core.CheckStateResult();
            }
            return ApiResultIoc.Ioc.DeserializeObject<TResult>(client.Result);
        }


        /// <summary>
        /// 计划调用(一次)
        /// </summary>
        /// <typeparam name="TArgument">参数类型</typeparam>
        /// <typeparam name="TResult">返回值类型</typeparam>
        /// <param name="station">站点</param>
        /// <param name="api">api名称</param>
        /// <param name="arg">参数</param>
        /// <param name="type">类型(除none time之外,基准时间为现在)</param>
        /// <param name="num">计划值</param>
        /// <param name="description">计划说明</param>
        /// <returns></returns>
        public static IApiResult<TResult> ApiPlan<TArgument, TResult>(string station, string api, TArgument arg, plan_date_type type,short num, string description)
        {
            var client = new ApiClient
            {
                Station = station,
                Commmand = api,
                Argument = arg == null ? null : JsonHelper.SerializeObject(arg)
            };

            var plan = new ZeroPlanInfo
            {
                plan_type = type,
                plan_value = num,
                plan_repet = 1,
                description = description,
                no_skip = true,
                skip_set = 0,
                plan_time = (int)((DateTime.Now.ToUniversalTime().Ticks - 621355968000000000) / 10000000)
            };
            using (MonitorScope.CreateScope("内部Zero调用"))
            {
                client.Prepare();
                client._core.Plan(plan);
                client.End();
            }
            if (client.State != ZeroOperatorStateType.Ok)
            {
                client._core.CheckStateResult();
            }
            return ApiResultIoc.Ioc.DeserializeObject<TResult>(client.Result);
        }
        #endregion
    }
}