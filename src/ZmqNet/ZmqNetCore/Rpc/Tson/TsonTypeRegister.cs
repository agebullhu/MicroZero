using System;
using System.Collections.Generic;

namespace Agebull.Common.DataModel
{
    /// <summary>
    /// Tson类型节点
    /// </summary>
    public class TsonTypeItem
    {
        /// <summary>
        /// 类型ID
        /// </summary>
        public int typeId;
        /// <summary>
        /// 名称
        /// </summary>
        public string name;
        /// <summary>
        /// 构造器
        /// </summary>
        public Func<ITson> creator;
    }
    /// <summary>
    /// Tson类型注册器
    /// </summary>
    public static class TsonTypeRegister
    {
        static readonly Dictionary<int, TsonTypeItem> types = new Dictionary<int, TsonTypeItem>();

        /// <summary>
        /// 注册类型
        /// </summary>
        /// <typeparam name="T">类型</typeparam>
        /// <param name="id">类型ID</param>
        /// <param name="name">名称</param>
        public static void RegisteType<T>(int id,string name) where T : ITson, new()
        {
            if (!types.ContainsKey(id))
            {
                types.Add(id, new TsonTypeItem
                {
                    typeId = id,
                    name = name,
                    creator = () => new T()
                });
            }
        }
        /// <summary>
        /// 注册类型
        /// </summary>
        /// <param name="id"></param>
        public static string TypeName(int id)
        {
            if (!types.ContainsKey(id))
                return null;
            return types[id].name;
        }
        /// <summary>
        /// 注册类型
        /// </summary>
        /// <param name="id"></param>
        public static ITson CreateType(int id)
        {
            if (!types.ContainsKey(id))
                return null;
            return types[id].creator();
        }
    }
}
