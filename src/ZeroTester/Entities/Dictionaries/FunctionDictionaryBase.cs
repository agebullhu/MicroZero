// /*****************************************************
// (c)2008-2013 Copy right www.Gboxt.com
// 作者:bull2
// 工程:CodeRefactor-Gboxt.Common.WpfMvvmBase
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
    ///     扩展方法字典
    /// </summary>
    /// <remarks>
    ///     依赖对象都为IgnoreDataMember属性,即不可网络序列化
    /// </remarks>
    [DataContract, JsonObject(MemberSerialization.OptIn)]
    public abstract class FunctionDictionaryBase
    {
        /// <summary>
        /// 有名称的方法字典
        /// </summary>
        [IgnoreDataMember]
        private readonly Dictionary<string, object> _nameDictionary = new Dictionary<string, object>();

        /// <summary>
        /// 依赖字典
        /// </summary>
        [IgnoreDataMember]
        private readonly Dictionary<Type, object> _dependencyDictionary = new Dictionary<Type, object>();

        /// <summary>
        ///     附加方法
        /// </summary>
        public void AnnexDelegate<TAction>(TAction action, string name = null) where TAction : class
        {
            if (string.IsNullOrEmpty(name))
            {
                var type = typeof(TAction);
                if (_dependencyDictionary.ContainsKey(type))
                {
                    if (Equals(action, null))
                    {
                        _dependencyDictionary.Remove(type);
                    }
                    else
                    {
                        _dependencyDictionary[type] = action;
                    }
                }
                else if (!Equals(action, null))
                {
                    _dependencyDictionary.Add(type, action);
                }
            }
            else if (_nameDictionary.ContainsKey(name))
            {
                if (Equals(action, null))
                {
                    _nameDictionary.Remove(name);
                }
                else
                {
                    _nameDictionary[name] = action;
                }
            }
            else if (!Equals(action, null))
            {
                _nameDictionary.Add(name, action);
            }
        }


        /// <summary>
        ///     取得内部附加对象
        /// </summary>
        /// <typeparam name="TAction">对象类型(理论上限定为Action或Func这两种类型的委托)</typeparam>
        /// <param name="name">方法名称</param>
        /// <returns>如果存在则返回方法,否则返回空</returns>
        public TAction GetDelegate<TAction>(string name = null)where TAction : class
        {
            object value;
            if (string.IsNullOrEmpty(name))
            {
                var type = typeof(TAction);
                if (!_dependencyDictionary.TryGetValue(type, out value))
                {
                    return null;
                }
            }
            else
            {
                if (!_nameDictionary.TryGetValue(name, out value))
                {
                    return null;
                }
            }
            if (!(value is TAction))
            {
                return null;
            }
            return (TAction)value;
        }

        /// <summary>
        ///     是否已附加对象
        /// </summary>
        /// <typeparam name="TAction">对象类型(理论上限定为Action或Func这两种类型的委托)</typeparam>
        /// <param name="name">方法名称</param>
        /// <returns>如果存在则返回方法,否则返回空</returns>
        public bool HaseDelegate<TAction>(string name = null) where TAction : class
        {
            return name == null
                ? _dependencyDictionary.ContainsKey( typeof(TAction)) 
                : _nameDictionary.ContainsKey(name);
        }
    }
}
