using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.Serialization;
using Agebull.Common.Reflection;
using Agebull.ZeroNet.Core;
using Newtonsoft.Json;

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
        /// 站点文档信息
        /// </summary>
        public Dictionary<string, StationDocument> StationInfo = new Dictionary<string, StationDocument>();
        public ZeroDiscover()
        {
            XmlMember.Load(this.GetType().Assembly);
        }
        StationDocument DefStation;

        /// <summary>
        /// 查找API
        /// </summary>
        public void FindApies()
        {
            XmlMember.Load(Assembly);
            StationInfo.Add(StationName, DefStation = new StationDocument
            {
                Name = StationName
            });
            var types = Assembly.GetTypes().Where(p => p.IsSubclassOf(typeof(ZeroApiController))).ToArray();
            foreach (var type in types)
            {
                FindApi(type);
            }
            RegistToZero();
        }
        void RegistToZero()
        {
            foreach (var sta in StationInfo.Values)
            {
                if (sta.Aips.Count == 0)
                    continue;
                var station = (ApiStation)ZeroApplication.TryGetZeroObject(sta.Name);
                if (station == null)
                {
                    ZeroApplication.RegistZeroObject(station = new ApiStation
                    {
                        Name = sta.Name,
                        StationName = sta.Name
                    });
                }
                foreach (var api in sta.Aips)
                {
                    var action = (ApiActionInfo)api.Value;
                    var a = action.HaseArgument
                        ? station.RegistAction(api.Key, action.ArgumentAction, action.AccessOption, action)
                        : station.RegistAction(api.Key, action.Action, action.AccessOption, action);
                    a.ArgumenType = action.ArgumenType;
                }
                if (!ZeroApplication.Config.Documents.TryGetValue(sta.Name, out var doc))
                {
                    ZeroApplication.Config.Documents.Add(sta.Name, sta);
                }
                else
                {
                    foreach (var api in sta.Aips)
                    {
                        if (!doc.Aips.ContainsKey(api.Key))
                        {
                            doc.Aips.Add(api.Key, api.Value);
                        }
                        else
                        {
                            doc.Aips[api.Key] = api.Value;
                        }
                    }
                }
            }
        }
        /// <summary>
        /// 查找API
        /// </summary>
        /// <param name="type"></param>
        private void FindApi(Type type)
        {
            StationDocument station;
            var sa = type.GetCustomAttribute<StationAttribute>();
            if (sa != null)
            {
                if (!StationInfo.TryGetValue(sa.Name, out station))
                {
                    StationInfo.Add(sa.Name, station = new StationDocument
                    {
                        Name = sa.Name
                    });
                }
            }
            else
            {
                station = DefStation;
            }
            station.Copy(XmlMember.Find(type));
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
                if (method.GetParameters().Length > 1)
                {
                    ZeroTrace.WriteError("ApiDiscover", "argument size must 0 or 1", station.Name, type.Name, method.Name);
                    continue;
                }
                var route = method.GetCustomAttribute<RouteAttribute>();
                if (route == null && !method.IsPublic)
                {
                    ZeroTrace.WriteError("ApiDiscover", "exclude", station.Name, type.Name, method.Name);
                    continue;
                }
                var name = route?.Name == null
                    ? $"{routeHead}{method.Name}"
                    : $"{routeHead}{route.Name.Trim(' ', '\t', '\r', '\n', '/')}";
                var accessOption = method.GetCustomAttribute<ApiAccessOptionFilterAttribute>();

                var api = new ApiActionInfo
                {
                    Name = method.Name,
                    RouteName = name,

                    Controller = type.FullName,
                    AccessOption = accessOption != null ? accessOption.Option : ApiAccessOption.Full,
                    ResultInfo = ReadEntity(method.ReturnType, "result")
                };
                station.Aips.Add(api.RouteName, api);
                var doc = XmlMember.Find(type, method.Name, "M");
                api.Copy(doc);

                var arg = method.GetParameters().FirstOrDefault();
                api.HaseArgument = arg != null;
                //动态生成并编译
                if (api.HaseArgument)
                {
                    api.ArgumenType = arg.ParameterType;
                    api.ArgumentInfo = ReadEntity(arg.ParameterType, "argument") ?? new TypeDocument();
                    api.ArgumentInfo.Name = arg.Name;
                    if (doc != null)
                        api.ArgumentInfo.Caption = doc.Arguments.Values.FirstOrDefault();

                    api.ArgumentAction = TypeExtend.CreateFunc<IApiArgument, IApiResult>(type.GetTypeInfo(),
                        method.Name,
                        arg.ParameterType.GetTypeInfo(),
                        method.ReturnType.GetTypeInfo());
                }
                else
                {
                    api.Action = TypeExtend.CreateFunc<IApiResult>(type.GetTypeInfo(), method.Name, method.ReturnType.GetTypeInfo());
                }
            }
        }

        #endregion

        #region ZeroObject发现

        /// <summary>
        /// 查找站点
        /// </summary>
        public void FindZeroObjects()
        {
            var types = Assembly.GetTypes().Where(p => p.IsSupperInterface(typeof(IZeroObject))).ToArray();
            foreach (var type in types)
            {
                ZeroApplication.RegistZeroObject(type.CreateObject() as IZeroObject);
            }
        }

        /// <summary>
        /// 查找API
        /// </summary>
        /// <param name="type"></param>
        public static void DiscoverApiDocument(Type type)
        {
            if (!type.IsSubclassOf(typeof(ApiStation)))
                return;
            ZeroDiscover discover = new ZeroDiscover();
            discover.FindApiDocument(type);
            foreach (var sta in discover.StationInfo.Values)
            {
                if (sta.Aips.Count == 0)
                    continue;
                if (!ZeroApplication.Config.Documents.TryGetValue(sta.Name, out var doc))
                {
                    ZeroApplication.Config.Documents.Add(sta.Name, sta);
                }
                else
                {
                    foreach (var api in sta.Aips)
                    {
                        if (!doc.Aips.ContainsKey(api.Key))
                        {
                            doc.Aips.Add(api.Key, api.Value);
                        }
                        else
                        {
                            doc.Aips[api.Key] = api.Value;
                        }
                    }
                }
            }
        }

        /// <summary>
        /// 查找API
        /// </summary>
        /// <param name="type"></param>
        private void FindApiDocument(Type type)
        {
            StationDocument station;
            var sa = type.GetCustomAttribute<StationAttribute>();
            if (sa != null)
            {
                if (!StationInfo.TryGetValue(sa.Name, out station))
                {
                    StationInfo.Add(sa.Name, station = new StationDocument
                    {
                        Name = sa.Name
                    });
                }
            }
            else
            {
                station = DefStation;
            }
            station.Copy(XmlMember.Find(type));
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
                if (method.GetParameters().Length > 1)
                {
                    ZeroTrace.WriteError("ApiDiscover", "argument size must 0 or 1", station.Name, type.Name, method.Name);
                    continue;
                }
                var route = method.GetCustomAttribute<RouteAttribute>();
                if (route == null && !method.IsPublic)
                {
                    ZeroTrace.WriteError("ApiDiscover", "exclude", station.Name, type.Name, method.Name);
                    continue;
                }
                var name = route?.Name == null
                    ? $"{routeHead}{method.Name}"
                    : $"{routeHead}{route.Name.Trim(' ', '\t', '\r', '\n', '/')}";
                var accessOption = method.GetCustomAttribute<ApiAccessOptionFilterAttribute>();

                var api = new ApiDocument
                {
                    Name = method.Name,
                    RouteName = name,
                    AccessOption = accessOption != null ? accessOption.Option : ApiAccessOption.Full,
                    ResultInfo = ReadEntity(method.ReturnType, "result")
                };
                station.Aips.Add(api.RouteName, api);
                var doc = XmlMember.Find(type, method.Name, "M");
                api.Copy(doc);
                var arg = method.GetParameters().FirstOrDefault();
                //动态生成并编译
                if (arg != null)
                {
                    api.ArgumentInfo = ReadEntity(arg.ParameterType, "argument") ?? new TypeDocument();
                    api.ArgumentInfo.Name = arg.Name;
                    if (doc != null)
                        api.ArgumentInfo.Caption = doc.Arguments.Values.FirstOrDefault();
                }
            }
        }
        #endregion

        #region XML文档
        private TypeDocument ReadEntity(Type type, string name)
        {
            if (type == typeof(void))
                return null;
            if (!IsLetter(type.Name[0]))
                return null;
            var typeDocument = new TypeDocument
            {
                Name = name,
                TypeName = ReflectionHelper.GetTypeName(type),
                ClassName = ReflectionHelper.GetTypeName(type),
                ObjectType = ObjectType.Object
            };
            typeDocument.Copy(XmlMember.Find(type));
            ReadEntity(typeDocument, type);
            return typeDocument;
        }
        bool IsLetter(char ch) => (ch >= 'a' && ch <= 'z') || (ch >= 'A' && ch <= 'Z');

        private void ReadEntity(TypeDocument typeDocument, Type type)
        {
            if (type == typeof(object))
                return;
            if (type.BaseType != typeof(object) && !type.BaseType.IsInterface)
                ReadEntity(typeDocument, type.BaseType);
            var dc = type.GetAttribute<DataContractAttribute>();
            var jo = type.GetAttribute<JsonObjectAttribute>();

            foreach (var property in type.GetProperties(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.DeclaredOnly))
            {
                if (property.IsSpecialName)
                {
                    continue;
                }
                CheckMember(typeDocument, type, property, property.PropertyType, jo != null, dc != null);
            }
            foreach (var field in type.GetFields(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.DeclaredOnly))
            {
                if (field.IsSpecialName)
                {
                    continue;
                }
                CheckMember(typeDocument, type, field, field.FieldType, jo != null, dc != null);
            }
        }
        void CheckMember(TypeDocument document, Type parent, MemberInfo member, Type memType, bool json, bool dc)
        {
            if (!IsLetter(member.Name[0]))
                return;
            var field = new TypeDocument();
            try
            {
                Type type = memType;
                if (memType.IsSubclassOf(typeof(IList<>)))
                {
                    type = type.GetGenericArguments()[0];
                }
                if (memType.IsSubclassOf(typeof(IDictionary<,>)))
                {
                    type = type.GetGenericArguments()[1];
                }
                if (memType.IsArray)
                {
                    type = type.MakeArrayType();
                }
                if (type.IsEnum)
                {
                    field = ReadEntity(type, member.Name);
                    field.ObjectType = ObjectType.Base;
                    field.IsEnum = type.IsEnum;
                }
                else if (type.IsBaseType())
                {
                    field.ObjectType = ObjectType.Base;
                    field.TypeName = ReflectionHelper.GetTypeName(type);
                }
                else
                {
                    field = ReadEntity(type, member.Name);
                    field.ObjectType = ObjectType.Object;
                }
            }
            catch
            {
                field.TypeName = "object";
            }
            field.Copy(XmlMember.Find(parent, member.Name, member is PropertyInfo ? "P" : "F"));
            field.Name = field.JsonName = member.Name;
            field.ClassName = ReflectionHelper.GetTypeName(memType);
            if (json)
            {
                var ji = member.GetAttribute<JsonIgnoreAttribute>();
                if (ji != null)
                {
                    return;
                }
                var jp = member.GetAttribute<JsonPropertyAttribute>();
                if (jp == null)
                    return;
                if (!string.IsNullOrWhiteSpace(jp.PropertyName))
                    field.JsonName = jp.PropertyName;
            }
            else if (dc)
            {
                var id = member.GetAttribute<IgnoreDataMemberAttribute>();
                if (id != null)
                {
                    return;
                }
                var dm = member.GetAttribute<DataMemberAttribute>();
                if (dm != null && !string.IsNullOrWhiteSpace(dm.Name))
                    field.JsonName = dm.Name;
            }
            if (memType.IsSubclassOf(typeof(IList<>)))
            {
                field.ObjectType = ObjectType.Array;
            }
            else if (memType.IsSubclassOf(typeof(IDictionary<,>)))
            {
                field.ObjectType = ObjectType.Dictionary;
            }
            else if (memType.IsArray)
            {
                field.ObjectType = ObjectType.Array;
            }
            document.Fields.Add(member.Name, field);
        }
        #endregion
    }
}