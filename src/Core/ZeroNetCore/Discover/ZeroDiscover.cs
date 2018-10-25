using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
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
            if (type.IsAbstract)
                return;
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
            else
            {
                var attrib2 = type.GetCustomAttribute<RoutePrefixAttribute>();
                if (attrib2 != null)
                {
                    routeHead = attrib2.Name;
                }
            }

            if (string.IsNullOrWhiteSpace(routeHead))
                routeHead = null;
            else
                routeHead = routeHead.Trim(' ', '\t', '\r', '\n', '/') + "/";

            var methods = type.GetMethods(BindingFlags.Instance
                                                                    | BindingFlags.Public
                                                                    | BindingFlags.NonPublic);

            foreach (var method in methods)
            {
                var route = method.GetCustomAttribute<RouteAttribute>();
                if (route == null)
                {
                    //ZeroTrace.WriteError("ApiDiscover", "exclude", station.Name, type.Name, method.Name);
                    continue;
                }
                if (method.Name.Length > 4 && (method.Name.IndexOf("get_") == 0 || method.Name.IndexOf("set_") == 0))
                    continue;
                if (method.GetParameters().Length > 1)
                {
                    //ZeroTrace.WriteError("ApiDiscover", "argument size must 0 or 1", station.Name, type.Name, method.Name);
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
                        api.ArgumentAction = CreateFunc<IApiArgument, IApiResult>(type.GetTypeInfo(),
                            method.Name,
                            arg.ParameterType.GetTypeInfo(),
                            method.ReturnType.GetTypeInfo());
                    }
                }
                else if (!onlyDoc)
                {
                    api.Action = CreateFunc<IApiResult>(type.GetTypeInfo(), method.Name, method.ReturnType.GetTypeInfo());
                }
                station.Aips.Add(api.RouteName, api);
            }
        }

        /// <summary>生成动态匿名调用内部方法（参数由TArg转为实际类型后调用，并将调用返回值转为TRes）</summary>
        /// <typeparam name="TArg">参数类型（接口）</typeparam>
        /// <typeparam name="TRes">返回值类型（接口）</typeparam>
        /// <param name="callInfo">调用对象类型</param>
        /// <param name="argInfo">原始参数类型</param>
        /// <param name="resInfo">原始返回值类型</param>
        /// <param name="methodName">原始调用方法</param>
        /// <returns>匿名委托</returns>
        public static Func<TArg, TRes> CreateFunc<TArg, TRes>(TypeInfo callInfo, string methodName, TypeInfo argInfo, TypeInfo resInfo)
        {
            ConstructorInfo constructor = callInfo.GetConstructor(Type.EmptyTypes);
            if (constructor == (ConstructorInfo)null)
                throw new ArgumentException("类型" + callInfo.FullName + "没有无参构造函数");
            MethodInfo method = callInfo.GetMethod(methodName);
            if (method == (MethodInfo)null)
                throw new ArgumentException("类型" + callInfo.FullName + "没有名称为" + methodName + "的方法");
            if (method.ReturnType != (Type)resInfo)
                throw new ArgumentException("类型" + callInfo.FullName + "的方法" + methodName + "返回值不为" + resInfo.FullName);
            ParameterInfo[] parameters = method.GetParameters();
            if (parameters.Length != 1)
                throw new ArgumentException("类型" + callInfo.FullName + "的方法" + methodName + "参数不是一个");
            if (parameters[0].ParameterType != (Type)argInfo)
                throw new ArgumentException("类型" + callInfo.FullName + "的方法" + methodName + "唯一参数不为" + argInfo.FullName);
            DynamicMethod dynamicMethod = new DynamicMethod(methodName, typeof(TRes), new Type[1]
            {
        typeof (TArg)
            });
            ILGenerator ilGenerator = dynamicMethod.GetILGenerator();
            ilGenerator.Emit(OpCodes.Nop);
            ilGenerator.Emit(OpCodes.Ldarg, 0);
            ilGenerator.Emit(OpCodes.Castclass, (Type)argInfo);
            LocalBuilder local1 = ilGenerator.DeclareLocal((Type)argInfo);
            ilGenerator.Emit(OpCodes.Stloc, local1);
            ilGenerator.Emit(OpCodes.Newobj, constructor);
            LocalBuilder local2 = ilGenerator.DeclareLocal((Type)callInfo);
            ilGenerator.Emit(OpCodes.Stloc, local2);
            ilGenerator.Emit(OpCodes.Ldloc, local2);
            ilGenerator.Emit(OpCodes.Ldloc, local1);
            ilGenerator.Emit(OpCodes.Callvirt, method);
            LocalBuilder local3 = ilGenerator.DeclareLocal(method.ReturnType);
            ilGenerator.Emit(OpCodes.Stloc, local3);
            ilGenerator.Emit(OpCodes.Ldloc, local3);
            ilGenerator.Emit(OpCodes.Castclass, (Type)typeof(TRes).GetTypeInfo());
            LocalBuilder local4 = ilGenerator.DeclareLocal((Type)resInfo);
            ilGenerator.Emit(OpCodes.Stloc, local4);
            ilGenerator.Emit(OpCodes.Ldloc, local4);
            ilGenerator.Emit(OpCodes.Ret);
            return dynamicMethod.CreateDelegate(typeof(Func<TArg, TRes>)) as Func<TArg, TRes>;
        }

        /// <summary>生成动态匿名调用内部方法（无参，调用返回值转为TRes）</summary>
        /// <typeparam name="TRes">返回值类型（接口）</typeparam>
        /// <param name="callInfo">调用对象类型</param>
        /// <param name="resInfo">原始返回值类型</param>
        /// <param name="methodName">原始调用方法</param>
        /// <returns>匿名委托</returns>
        public static Func<TRes> CreateFunc<TRes>(TypeInfo callInfo, string methodName, TypeInfo resInfo)
        {
            ConstructorInfo constructor = callInfo.GetConstructor(Type.EmptyTypes);
            if (constructor == (ConstructorInfo)null)
                throw new ArgumentException("类型" + callInfo.FullName + "没有无参构造函数");
            MethodInfo method = callInfo.GetMethod(methodName);
            if (method == (MethodInfo)null)
                throw new ArgumentException("类型" + callInfo.FullName + "没有名称为" + methodName + "的方法");
            if (method.ReturnType != (Type)resInfo)
                throw new ArgumentException("类型" + callInfo.FullName + "的方法" + methodName + "返回值不为" + resInfo.FullName);
            if ((uint)method.GetParameters().Length > 0U)
                throw new ArgumentException("类型" + callInfo.FullName + "的方法" + methodName + "参数不为空");
            DynamicMethod dynamicMethod = new DynamicMethod(methodName, typeof(TRes), (Type[])null);
            ILGenerator ilGenerator = dynamicMethod.GetILGenerator();
            ilGenerator.Emit(OpCodes.Nop);
            ilGenerator.Emit(OpCodes.Newobj, constructor);
            LocalBuilder local1 = ilGenerator.DeclareLocal((Type)callInfo);
            ilGenerator.Emit(OpCodes.Stloc, local1);
            ilGenerator.Emit(OpCodes.Ldloc, local1);
            ilGenerator.Emit(OpCodes.Callvirt, method);
            LocalBuilder local2 = ilGenerator.DeclareLocal(method.ReturnType);
            ilGenerator.Emit(OpCodes.Stloc, local2);
            ilGenerator.Emit(OpCodes.Ldloc, local2);
            ilGenerator.Emit(OpCodes.Castclass, (Type)typeof(TRes).GetTypeInfo());
            LocalBuilder local3 = ilGenerator.DeclareLocal((Type)resInfo);
            ilGenerator.Emit(OpCodes.Stloc, local3);
            ilGenerator.Emit(OpCodes.Ldloc, local3);
            ilGenerator.Emit(OpCodes.Ret);
            return dynamicMethod.CreateDelegate(typeof(Func<TRes>)) as Func<TRes>;
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
        bool IsLetter(char ch) => (ch >= 'a' && ch <= 'z') || (ch >= 'A' && ch <= 'Z');

        private Dictionary<Type, TypeDocument> typeDocs2 = new Dictionary<Type, TypeDocument>();

        private Dictionary<Type, TypeDocument> typeDocs = new Dictionary<Type, TypeDocument>();
        private TypeDocument ReadEntity(Type type, string name)
        {
            var typeDocument = new TypeDocument
            {
                Name = name,
                TypeName = ReflectionHelper.GetTypeName(type),
                ClassName = ReflectionHelper.GetTypeName(type),
                ObjectType = ObjectType.Object
            };

            //if (typeDocs.TryGetValue(type, out var doc))
            //    return doc;
            ReadEntity(typeDocument, type);
            return typeDocument;
        }
        private void ReadEntity(TypeDocument typeDocument, Type type)
        {
            if (type == null || type.IsAutoClass || !IsLetter(type.Name[0]) ||
                type.IsInterface || type.IsMarshalByRef || type.IsCOMObject ||
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
            if (typeDocs2.TryGetValue(type, out var _))
            {
                ZeroTrace.WriteError("ReadEntity", "over flow", type.Name);

                return;
            }

            typeDocs2.Add(type, typeDocument);
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
                        info.TypeName = "int";
                        info.Example = ((int)field.GetValue(null)).ToString();
                        info.JsonName = null;
                    }
                }
                typeDocs.Add(type, new TypeDocument
                {
                    fields = typeDocument.fields?.ToDictionary(p => p.Key, p => p.Value)
                });
                typeDocument.Copy(XmlMember.Find(type));
                return;
            }

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
                if (!char.IsLetter(field.Name[0]) || field.IsSpecialName)
                {
                    continue;
                }
                CheckMember(typeDocument, type, field, field.FieldType, jo != null, dc != null);
            }

            typeDocs.Add(type, new TypeDocument
            {
                fields = typeDocument.fields?.ToDictionary(p => p.Key, p => p.Value)
            });
            ReadEntity(typeDocument, type.BaseType);
            typeDocument.Copy(XmlMember.Find(type));
        }
        TypeDocument CheckMember(TypeDocument document, Type parent, MemberInfo member, Type memType, bool json, bool dc, bool checkBase = true)
        {
            if (document.Fields.ContainsKey(member.Name))
                return null;
            var field = new TypeDocument
            {
                Name = member.Name,
                JsonName = member.Name,
                ClassName = ReflectionHelper.GetTypeName(memType)
            };
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

            var doc = XmlMember.Find(parent, member.Name);
            field.Copy(doc);
            bool isArray = false;
            bool isDictionary = false;
            try
            {
                Type type = memType;
                if (memType.IsArray)
                {
                    isArray = true;
                    type = type.Assembly.GetType(type.FullName.Split('[')[0]);
                }
                else if (type.IsGenericType)
                {
                    if (memType.IsSupperInterface(typeof(ICollection<>)))
                    {
                        isArray = true;
                        type = type.GetGenericArguments()[0];
                    }
                    else if (memType.IsSupperInterface(typeof(IDictionary<,>)))
                    {
                        var fields = type.GetGenericArguments();
                        field.Fields.Add("Key", ReadEntity(fields[0], "Key"));
                        field.Fields.Add("Value", ReadEntity(fields[1], "Value"));
                        field.ObjectType = ObjectType.Object;
                        isDictionary = true;
                        checkBase = false;
                    }
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
                }
                else
                {
                    if (checkBase)
                        field = ReadEntity(type, member.Name);
                    field.ObjectType = ObjectType.Object;
                }
                field.TypeName = ReflectionHelper.GetTypeName(type);
            }
            catch
            {
                field.TypeName = "object";
            }
            if (isArray)
            {
                field.TypeName += "[]";
                field.ObjectType = ObjectType.Array;
            }
            else if (isDictionary)
            {
                field.TypeName = "Dictionary";
                field.ObjectType = ObjectType.Dictionary;
            }
            document.Fields.Add(member.Name, field);

            return field;
        }
        #endregion
    }
}