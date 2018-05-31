using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Agebull.ZeroNet.Core;
using Agebull.ZeroNet.PubSub;

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
        public Assembly Assembly;
        
        /// <summary>
        /// 查找API
        /// </summary>
        public void FindApies()
        {
            var types = Assembly.GetTypes().Where( p=>p.IsSubclassOf(typeof(ZeroApiController))).ToArray();
            foreach (var type in types)
            {
                FindApi(type);
            }
        }
        /// <summary>
        /// 查找API
        /// </summary>
        /// <param name="type"></param>
        private void FindApi(Type  type)
        {
            var methods = type.GetMethods(BindingFlags.DeclaredOnly | BindingFlags.Instance 
                                                                    | BindingFlags.Public 
                                                                    | BindingFlags.NonPublic);
            var apiItems = new Dictionary<string, ApiActionInfo>(StringComparer.OrdinalIgnoreCase);

            foreach (var method in methods)
            {
                var route = method.GetCustomAttribute<RouteAttribute>();
                if (route == null)
                    continue;
                var info = new ApiActionInfo
                {
                    FunctionName = method.Name,
                    RouteName = route.Name
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
                    
                    info.ArgumentAction = TypeExtend.CreateFunc<IApiArgument, IApiResult>(type.GetTypeInfo(), method.Name,pars[0].ParameterType.GetTypeInfo(), method.ReturnType.GetTypeInfo());
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
            if (apiItems.Count == 0)
                return;
            var station = new ApiStation
            {
                StationName = ZeroApplication.Config.StationName
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