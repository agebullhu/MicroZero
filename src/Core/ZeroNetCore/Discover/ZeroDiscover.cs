using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Reflection;
using System.Runtime.Serialization;
using Agebull.Common.ApiDocuments;
using Agebull.Common.Reflection;
using Agebull.Common.Rpc;
using Agebull.ZeroNet.Core;
using Gboxt.Common.DataModel;
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
            XmlMember.Load(GetType().Assembly);
        }

        private StationDocument _defStation;

        /// <summary>
        /// 查找API
        /// </summary>
        public void FindApies()
        {
            XmlMember.Load(Assembly);
            StationInfo.Add(StationName, _defStation = new StationDocument
            {
                Name = StationName
            });
            var types = Assembly.GetTypes().Where(p => p.IsSubclassOf(typeof(ApiController))).ToArray();
            foreach (var type in types)
            {
                FindApi(type, false);
            }
            RegistToZero();

            RegistDocument();
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
            }
        }
        /// <summary>
        /// 查找API
        /// </summary>
        /// <param name="type"></param>
        /// <param name="onlyDoc"></param>
        private void FindApi(Type type, bool onlyDoc)
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
                station = _defStation;
            }
            var xdoc = XmlMember.Find(type);
            //station.Copy(XmlMember.Find(type));
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
                var ca = method.GetAttribute<CategoryAttribute>();
                var api = new ApiActionInfo
                {
                    Name = method.Name,
                    RouteName = name,
                    Category = ca?.Category ?? xdoc?.Caption,
                    Controller = type.FullName,
                    AccessOption = accessOption?.Option ?? ApiAccessOption.Public | ApiAccessOption.Anymouse | ApiAccessOption.ArgumentCanNil,
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
                    api.ArgumentInfo = ReadEntity(arg.ParameterType, "argument") ?? new TypeDocument();
                    api.ArgumentInfo.Name = arg.Name;
                    if (doc != null)
                        api.ArgumentInfo.Caption = doc.Arguments.Values.FirstOrDefault();

                    if (!onlyDoc)
                    {
                        api.ArgumenType = arg.ParameterType;
                        api.ArgumentAction = TypeExtend.CreateFunc<IApiArgument, IApiResult>(type.GetTypeInfo(),
                            method.Name,
                            arg.ParameterType.GetTypeInfo(),
                            method.ReturnType.GetTypeInfo());
                    }
                }
                else if (!onlyDoc)
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
            ZeroTrace.SystemLog("FindZeroObjects", Assembly.Location);
            Type[] types;
            try
            {
                types = Assembly.GetTypes().Where(p => p.IsSupperInterface(typeof(IZeroObject))).ToArray();
            }
            catch (ReflectionTypeLoadException ex)
            {
                types = ex.Types.Where(p => p != null).ToArray();
                ZeroTrace.WriteException("FindZeroObjects:GetTypes", ex);
            }
            try
            {
                foreach (var type in types)
                {
                    ZeroApplication.RegistZeroObject(type.CreateObject() as IZeroObject);
                }
            }
            catch (Exception ex)
            {
                ZeroTrace.WriteException("FindZeroObjects:RegistZeroObject", ex);
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
            ZeroTrace.SystemLog("DiscoverApiDocument", type.FullName);
            ZeroDiscover discover = new ZeroDiscover();
            discover.FindApi(type, true);
            discover.RegistDocument();
        }
        #endregion

        #region XML文档
        void RegistDocument()
        {
            foreach (var sta in StationInfo.Values)
            {
                if (sta.Aips.Count == 0)
                    continue;
                if (!ZeroApplication.Config.Documents.TryGetValue(sta.Name, out var doc))
                {
                    ZeroApplication.Config.Documents.Add(sta.Name, sta);
                    continue;
                }
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

        private Dictionary<Type, TypeDocument> typeDocs = new Dictionary<Type, TypeDocument>();
        private TypeDocument ReadEntity(Type type, string name)
        {
            if (type == typeof(void))
                return null;
            if (!IsLetter(type.Name[0]))
                return null;
            if (typeDocs.TryGetValue(type, out var doc))
                return doc;
            var typeDocument = new TypeDocument
            {
                Name = name,
                TypeName = ReflectionHelper.GetTypeName(type),
                ClassName = ReflectionHelper.GetTypeName(type),
                ObjectType = ObjectType.Object
            };
            ReadEntity(typeDocument, type);
            return typeDocument;
        }
        bool IsLetter(char ch) => (ch >= 'a' && ch <= 'z') || (ch >= 'A' && ch <= 'Z');

        private void ReadEntity(TypeDocument typeDocument, Type type)
        {
            if (type == null || type.IsAutoClass || type.IsInterface || type.IsMarshalByRef || type.IsCOMObject ||
                type == typeof(object) || type == typeof(void) ||
                type == typeof(ValueType) || type == typeof(Type) || type == typeof(Enum) ||
                type.Namespace == "System" || type.Namespace?.Contains("System.") == true)
                return;
            if (typeDocs.TryGetValue(type, out var doc))
            {
                foreach (var field in doc.Fields)
                {
                    if (typeDocument.Fields.ContainsKey(field.Key))
                        typeDocument.Fields[field.Key] = field.Value;
                    else
                        typeDocument.Fields.Add(field.Key, field.Value);
                }
                return;
            }
            if (type.IsArray)
            {
                ReadEntity(typeDocument, type.Assembly.GetType(type.FullName.Split('[')[0]));
                return;
            }
            if (type.IsGenericType && !type.IsValueType &&
                type.GetGenericTypeDefinition().GetInterface(typeof(IEnumerable<>).FullName) != null)
            {
                ReadEntity(typeDocument, type.GetGenericArguments().Last());
                return;
            }

            XmlMember.Find(type);
            typeDocument.Copy(XmlMember.Find(type));
            if (type.IsEnum)
            {
                foreach (var field in type.GetFields(BindingFlags.Static | BindingFlags.Public))
                {
                    if (field.IsSpecialName)
                    {
                        continue;
                    }
                    var info = CheckMember(typeDocument, type, field, field.FieldType, false, false, false);
                    if (info != null)
                    {
                        info.TypeName = "const";
                        info.Value = ((int)field.GetValue(null)).ToString();
                    }
                }
            }
            else
            {
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

            typeDocs.Add(type, new TypeDocument
            {
                fields = typeDocument.fields.ToDictionary(p => p.Key, p => p.Value)
            });
        }
        TypeDocument CheckMember(TypeDocument document, Type parent, MemberInfo member, Type memType, bool json, bool dc, bool checkBase = true)
        {
            if (!IsLetter(member.Name[0]) || document.Fields.ContainsKey(member.Name))
                return null;
            var field = new TypeDocument
            {
                Name = member.Name,
                JsonName = member.Name,
                ClassName = ReflectionHelper.GetTypeName(memType)
            };
            var doc = XmlMember.Find(parent, member.Name);
            field.Copy(doc);
            try
            {
                Type type = memType;
                if (memType.IsArray)
                {
                    type = type.Assembly.GetType(type.FullName.Split('[')[0]);
                }
                if (memType.IsSubclassOf(typeof(ICollection<>)))
                {
                    type = type.GetGenericArguments()[0];
                }
                if (memType.IsSubclassOf(typeof(IDictionary<,>)))
                {
                    type = type.GetGenericArguments()[1];
                }
                if (type.IsEnum)
                {
                    if (checkBase)
                        field = ReadEntity(type, member.Name);
                    field.ObjectType = ObjectType.Base;
                    field.IsEnum = true;
                }
                else if (type.IsBaseType())
                {
                    field.ObjectType = ObjectType.Base;
                    field.TypeName = ReflectionHelper.GetTypeName(type);
                }
                else
                {
                    if (checkBase)
                        field = ReadEntity(type, member.Name);
                    field.ObjectType = ObjectType.Object;
                }
            }
            catch
            {
                field.TypeName = "object";
            }
            if (json)
            {
                var ji = member.GetAttribute<JsonIgnoreAttribute>();
                if (ji != null)
                {
                    return null;
                }
                var jp = member.GetAttribute<JsonPropertyAttribute>();
                if (jp == null)
                    return null;
                if (!string.IsNullOrWhiteSpace(jp.PropertyName))
                    field.JsonName = jp.PropertyName;
            }
            else if (dc)
            {
                var id = member.GetAttribute<IgnoreDataMemberAttribute>();
                if (id != null)
                    return null;
                var dm = member.GetAttribute<DataMemberAttribute>();
                if (dm != null && !string.IsNullOrWhiteSpace(dm.Name))
                    field.JsonName = dm.Name;
            }
            if (memType.IsSubclassOf(typeof(ICollection<>)))
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
            var rule = member.GetAttribute<DataRuleAttribute>();
            if (rule != null)
            {
                field.CanNull = rule.CanNull;
                field.Regex = rule.Regex;
                if (rule.Min != long.MinValue)
                    field.Min = rule.Min;
                if (rule.Max != long.MinValue)
                    field.Max = rule.Max;
                if (rule.MinDate != DateTime.MinValue)
                    field.MinDate = rule.MinDate;
                if (rule.MaxDate != DateTime.MaxValue)
                    field.MaxDate = rule.MaxDate;
            }
            document.Fields.Add(member.Name, field);
            return field;
        }
        #endregion
    }
}