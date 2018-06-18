using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Agebull.ZeroNet.Core;

namespace Agebull.ZeroNet.ZeroApi
{
    /// <summary>
    /// ZeroStation发现工具
    /// </summary>
    internal class ZeroDiscover
    {
        #region API发现
        /// <summary>
        /// 主调用程序集
        /// </summary>
        public Assembly Assembly { get; set; }

        public string StationName { get; set; }
        /// <summary>
        ///     Api方法的信息
        /// </summary>
        class ApiActionInfo
        {
            /// <summary>
            ///     方法名称
            /// </summary>
            public string FunctionName;

            /// <summary>
            ///     Api路由名称
            /// </summary>
            public string RouteName;

            /// <summary>
            ///     访问设置
            /// </summary>
            public ApiAccessOption AccessOption;

            /// <summary>
            ///     是否有调用参数
            /// </summary>
            public bool HaseArgument;

            /// <summary>
            ///     无参方法
            /// </summary>
            public Func<IApiResult> Action;

            /// <summary>
            ///     有参方法
            /// </summary>
            public Func<IApiArgument, IApiResult> ArgumentAction;

            /// <summary>
            ///     参数类型
            /// </summary>
            public Type ArgumenType;
        }
        /// <summary>
        /// 查找API
        /// </summary>
        public void FindApies()
        {
            var apiItems = new Dictionary<string, ApiActionInfo>(StringComparer.OrdinalIgnoreCase);
            var types = Assembly.GetTypes().Where(p => p.IsSubclassOf(typeof(ZeroApiController))).ToArray();
            foreach (var type in types)
            {
                FindApi(type, apiItems);
            }
            if (apiItems.Count == 0)
                return;
            var station = new ApiStation
            {
                Name = StationName,
                StationName = StationName
            };
            foreach (var action in apiItems)
            {
                var a = action.Value.HaseArgument
                    ? station.RegistAction(action.Key, action.Value.ArgumentAction, action.Value.AccessOption)
                    : station.RegistAction(action.Key, action.Value.Action, action.Value.AccessOption);
                a.ArgumenType = action.Value.ArgumenType;
            }
            ZeroApplication.RegistZeroObject(station);
        }

        /// <summary>
        /// 查找API
        /// </summary>
        /// <param name="type"></param>
        /// <param name="apiItems"></param>
        private void FindApi(Type type, Dictionary<string, ApiActionInfo> apiItems)
        {
            string routeHead = null;
            var attrib = type.GetCustomAttribute<RouteAttribute>();
            if (attrib != null)
            {
                routeHead = attrib.Name;
            }

            if (string.IsNullOrWhiteSpace(routeHead))
                routeHead = null;
            else
                routeHead = routeHead.Trim(' ', '\t', '\r', '\n', '/') + "/";

            var methods = type.GetMethods(BindingFlags.DeclaredOnly | BindingFlags.Instance
                                                                    | BindingFlags.Public
                                                                    | BindingFlags.NonPublic);

            foreach (var method in methods)
            {
                var route = method.GetCustomAttribute<RouteAttribute>();
                if (route == null)
                    continue;
                var name = route.Name == null 
                    ? $"{routeHead}{method.Name}" 
                    : $"{routeHead}{route.Name.Trim(' ', '\t', '\r', '\n', '/')}";

                var info = new ApiActionInfo
                {
                    FunctionName = method.Name,
                    RouteName = name
                };
                var pars = method.GetParameters();
                if (method.GetParameters().Length == 0)
                {
                    info.HaseArgument = false;
                    info.Action = TypeExtend.CreateFunc<IApiResult>(type.GetTypeInfo(), method.Name, method.ReturnType.GetTypeInfo());
                }
                else if (method.GetParameters().Length == 1)
                {
                    info.HaseArgument = true;
                    info.ArgumentAction = TypeExtend.CreateFunc<IApiArgument, IApiResult>(type.GetTypeInfo(), method.Name, pars[0].ParameterType.GetTypeInfo(), method.ReturnType.GetTypeInfo());
                    info.ArgumenType = pars[0].ParameterType;
                }
                else
                {
                    continue;
                }
                var accessOption = method.GetCustomAttribute<ApiAccessOptionFilterAttribute>();
                if (accessOption != null)
                    info.AccessOption = accessOption.Option;
                apiItems.Add(info.RouteName, info);
            }
        }

        #endregion

        #region ZeroObject发现

        /// <summary>
        /// 查找API
        /// </summary>
        public void FindZeroObjects()
        {
            var types = Assembly.GetTypes().Where(p => p.IsSupperInterface(typeof(IZeroObject))).ToArray();
            foreach (var type in types)
            {
                ZeroApplication.RegistZeroObject(type.CreateObject() as IZeroObject);
            }
        }

        #endregion

    }
}