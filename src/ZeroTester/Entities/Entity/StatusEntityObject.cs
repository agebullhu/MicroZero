// /*****************************************************
// (c)2008-2013 Copy right www.Gboxt.com
// 作者:bull2
// 工程:CodeRefactor-Agebull.Common.WpfMvvmBase
// 建立:2014-12-09
// 修改:2014-12-09
// *****************************************************/

#region 引用

using System.ComponentModel;
using System.Runtime.Serialization;
using Newtonsoft.Json;

#endregion

namespace Agebull.Common.DataModel
{
    /// <summary>
    ///     表示表示网络对象
    /// </summary>
    [DataContract, JsonObject(MemberSerialization.OptIn)]
    public abstract class StatusEntityObject<TStatus> : EntityObjectBase, INetObject, IExtendAttribute, IExtendDependencyObjects,IExtendDependencyDelegates,IExtendDelegates 
            where TStatus : SerializableStatus, new()
    {
        #region 状态对象

        /// <summary>
        ///     状态对象
        /// </summary>
        [DataMember]
        private TStatus _status;

        /// <summary>
        ///     状态对象
        /// </summary>
        [Browsable(false), IgnoreDataMember]
        public TStatus __EntityStatus
        {
            get
            {
                return this._status ?? ( this._status = this.CreateStatus() );
            }
        }

        /// <summary>
        ///     构建状态对象
        /// </summary>
        protected virtual TStatus CreateStatus()
        {
            var status = new TStatus();
            status.Initialize(this);
            return status;
        }

        #endregion

        #region 序列化状态

        /// <summary>
        ///     表示网路对象来源
        /// </summary>
        NetObjectSource INetObject.NetObjectSource
        {
            get
            {
                return this.__EntityStatus.ObjectSource;
            }
            set
            {
                this.__EntityStatus.ObjectSource = value;
            }
        }

        /// <summary>
        ///     序列化中
        /// </summary>
        bool INetObject.IsSerializing
        {
            get
            {
                return this.__EntityStatus.IsSerializing;
            }
            set
            {
                this.__EntityStatus.IsSerializing = value;
            }
        }

        /// <summary>
        ///     反序列化中
        /// </summary>
        bool INetObject.IsDeserializing
        {
            get
            {
                return this.__EntityStatus.IsDeserializing;
            }
            set
            {
                this.__EntityStatus.IsDeserializing = value;
            }
        }

        /// <summary>
        ///     开始反序列化时的处理
        /// </summary>
        [OnDeserializing]
        private void OnDeserializing(StreamingContext context)
        {
            this.__EntityStatus.IsDeserializing = true;
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
            this.__EntityStatus.IsDeserializing = false;
            this.OnDeserialized();
        }

        /// <summary>
        ///     修复并尽量保证运行时的正确性
        /// </summary>
        [OnSerializing]
        protected virtual void OnSerializing(StreamingContext context)
        {
            this.__EntityStatus.IsSerializing = true;
            this.OnSerializing();
        }

        /// <summary>
        ///     完成序列化的处理
        /// </summary>
        [OnSerialized]
        protected virtual void OnSerialized(StreamingContext context)
        {
            this.__EntityStatus.IsSerializing = false;
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

        #region 扩展对象

        AttributeDictionary IExtendAttribute.Attribute
        {
            get
            {
                return __EntityStatus.Attribute;
            }
        }

        DependencyObjects IExtendDependencyObjects.Dependency
        {
            get
            {
                return __EntityStatus.DependencyObjects;
            }
        }

        DependencyDelegates IExtendDependencyDelegates.Delegates
        {
            get
            {
                return __EntityStatus.DependencyDelegates;
            }
        }

        IFunctionDictionary IExtendDelegates.Delegates
        {
            get
            {
                return __EntityStatus.ModelFunction;
            }
        }
        #endregion
    }

    /// <summary>
    ///     表示表示网络对象
    /// </summary>
    [DataContract, JsonObject(MemberSerialization.OptIn)]
    public abstract class StatusEntityObject : StatusEntityObject<SerializableStatus>
    {
    }
}
