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
    public class ZeroStationDiscover
    {
        #region API发现

        /// <summary>
        /// 所有API方法的基本信息
        /// </summary>
        public Dictionary<string, ApiActionInfo> ApiItems = new Dictionary<string, ApiActionInfo>(StringComparer.OrdinalIgnoreCase);

        /// <summary>
        /// 查找API
        /// </summary>
        /// <param name="assembly"></param>
        public void FindApies(Assembly assembly)
        {
            var types = assembly.GetTypes().Where( p=>p.IsSubclassOf(typeof(ZeroApiController))).ToArray();
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
                }
                else
                {
                    continue;
                }
                var accessOption = method.GetCustomAttribute<ApiAccessOptionFilterAttribute>();
                if (accessOption != null)
                    info.AccessOption = accessOption.Option;
                ApiItems.Add(info.RouteName, info);
            }
        }

        #endregion

        #region API发现

        /// <summary>
        /// 所有API方法的基本信息
        /// </summary>
        public Dictionary<string, TypeInfo> SubItems = new Dictionary<string, TypeInfo>(StringComparer.OrdinalIgnoreCase);

        /// <summary>
        /// 查找API
        /// </summary>
        /// <param name="assembly"></param>
        public void FindSubs(Assembly assembly)
        {
            var types = assembly.GetTypes().Where(p => p.IsSubclassOf(typeof(SubStation))).ToArray();
            foreach (var type in types)
            {
                StationProgram.RegisteStation(type.CreateObject() as SubStation);
            }
        }

        #endregion
    }
}