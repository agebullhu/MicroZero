// /*****************************************************
// (c)2008-2013 Copy right www.Agebull.com
// 作者:bull2
// 工程:CodeRefactor-Agebull.Common.WpfMvvmBase
// 建立:2014-11-29
// 修改:2014-11-29
// *****************************************************/

#region 引用

using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using Newtonsoft.Json;

#endregion

namespace Agebull.EntityModel
{
    /// <summary>
    ///     对象依赖字典
    /// </summary>
    /// <remarks>
    ///     依赖对象都为IgnoreDataMember属性,即不可网络序列化
    /// </remarks>
    [DataContract, JsonObject(MemberSerialization.OptIn)]
    public sealed class DependencyObjects
    {
        [DataMember]
        private readonly Dictionary<Type, object> _dictionary = new Dictionary<Type, object>();

        /// <summary>
        ///     附加一种类型对象
        /// </summary>
        /// <remarks>
        ///     这种方法只存在一个,即多次附加,只存最后一个对象
        /// </remarks>
        public void Annex<T>(T value)
        {
            Type type = typeof (T);
            if (_dictionary.ContainsKey(type))
            {
                if (Equals(value, default( T )))
                {
                    _dictionary.Remove(type);
                }
                else
                {
                    _dictionary[type] = value;
                }
            }
            else if (!Equals(value, default( T )))
            {
                _dictionary.Add(type, value);
            }
        }

        /// <summary>
        ///     取得一种类型的扩展属性(可以自动构造或提前附加)
        /// </summary>
        /// <returns></returns>
        public T AutoDependency<T>() where T : class, new()
        {
            if (_dictionary.TryGetValue(typeof(T), out object value1))
            {
                return value1 as T;
            }
            T value = new T();
            _dictionary.Add(typeof (T), value);
            return value;
        }
        /// <summary>
        ///     取得一种类型的扩展属性(需要附加)
        /// </summary>
        /// <returns></returns>
        public T Dependency<T>() where T : class
        {
            return _dictionary.TryGetValue(typeof(T), out object value1) ? value1 as T : null;
        }
    }
}
