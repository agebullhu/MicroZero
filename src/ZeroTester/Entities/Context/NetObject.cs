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

namespace Agebull.Common.DataModel
{
    /// <summary>
    ///     表示表示网络对象
    /// </summary>
    [DataContract, JsonObject(MemberSerialization.OptIn)]
    public class NetObject : INetObject
    {
        #region 已知类型注册

        /// <summary>
        ///     登记已知类型
        /// </summary>
        private static readonly List<Type> typeList = new List<Type>();

        /// <summary>
        ///     登记已知类型
        /// </summary>
        private static Type[] _types;

        /// <summary>
        ///     又注册了新类型(防止频繁的ToArray分配内存)
        /// </summary>
        private static bool _haseNew;

        /// <summary>
        ///     取得所有派生的类型
        /// </summary>
        /// <returns> </returns>
        public static Type[] KnownTypes
        {
            get
            {
                return _haseNew ? ( _types = typeList.ToArray() ) : _types;
            }
        }

        /// <summary>
        ///     加入继承类型到已知类型以便于正确序列化
        /// </summary>
        /// <param name="type"> </param>
        public static void RegisteSupperType(Type type)
        {
            if (typeList.Contains(type))
            {
                return;
            }
            typeList.Add(type);
            _haseNew = true;
        }

        /// <summary>
        ///     加入继承类型到已知类型以便于正确序列化
        /// </summary>
        public static void RegisteSupperType<T>()
        {
            RegisteSupperType(typeof (T));
        }

        /// <summary>
        ///     取得所有派生的类型
        /// </summary>
        /// <returns> </returns>
        public static Type[] GetKnownTypes()
        {
            return KnownTypes;
        }

        #endregion

        #region WCF序列化状态

        /// <summary>
        ///     表示网路对象来源
        /// </summary>
        [DataMember(Name = "__NetObjectSource")]
        public NetObjectSource __NetObjectSource
        {
            get;
            set;
        }

        /// <summary>
        ///     序列化中
        /// </summary>
        public bool __IsSerializing
        {
            get;
            private set;
        }

        /// <summary>
        ///     反序列化中
        /// </summary>
        public bool __IsDeserializing
        {
            get;
            private set;
        }

        /// <summary>
        ///     表示网路对象来源
        /// </summary>
        NetObjectSource INetObject.NetObjectSource
        {
            get
            {
                return this.__NetObjectSource;
            }
            set
            {
                this.__NetObjectSource = value;
            }
        }

        /// <summary>
        ///     序列化中
        /// </summary>
        bool INetObject.IsSerializing
        {
            get
            {
                return this.__IsSerializing;
            }
            set
            {
                this.__IsSerializing = value;
            }
        }

        /// <summary>
        ///     反序列化中
        /// </summary>
        bool INetObject.IsDeserializing
        {
            get
            {
                return this.__IsDeserializing;
            }
            set
            {
                this.__IsDeserializing = value;
            }
        }

        /// <summary>
        ///     开始反序列化时的处理
        /// </summary>
        [OnDeserializing]
        private void OnDeserializing(StreamingContext context)
        {
            this.__IsDeserializing = true;
            this.OnDeserializing();
        }

        /// <summary>
        ///     完成反序列化时的处理
        /// </summary>
        [OnDeserialized]
        private void OnDeserialized(StreamingContext context)
        {
#if SERVICE
            IsFromClient = true;
#endif
            this.__IsDeserializing = false;
            this.OnDeserialized();
        }

        /// <summary>
        ///     修复并尽量保证运行时的正确性
        /// </summary>
        [OnSerializing]
        protected virtual void OnSerializing(StreamingContext context)
        {
            this.__IsSerializing = true;
            this.OnSerializing();
        }

        /// <summary>
        ///     完成序列化的处理
        /// </summary>
        [OnSerialized]
        protected virtual void OnSerialized(StreamingContext context)
        {
            this.__IsSerializing = false;
            this.OnSerialized();
        }

        #endregion

        #region 序列化重载

        /// <summary>
        ///     正在进行反序列化
        /// </summary>
        protected virtual void OnDeserializing()
        {
        }

        /// <summary>
        ///     完成反序列化
        /// </summary>
        protected virtual void OnDeserialized()
        {
        }

        /// <summary>
        ///     正在进行序列化
        /// </summary>
        protected virtual void OnSerializing()
        {
        }

        /// <summary>
        ///     完成序列化的处理
        /// </summary>
        protected virtual void OnSerialized()
        {
        }

        #endregion
    }
}
