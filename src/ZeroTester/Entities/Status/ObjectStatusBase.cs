using System.Runtime.Serialization;
using Newtonsoft.Json;

namespace Agebull.Common.DataModel
{
    /// <summary>
    /// 对象状态基类
    /// </summary>
    [DataContract, JsonObject(MemberSerialization.OptIn)]
    public class ObjectStatusBase
    {
        /// <summary>
        /// 对应的对象
        /// </summary>
        [IgnoreDataMember]
        public NotificationObject Object { get; private set; }

        /// <summary>
        /// 初始化
        /// </summary>
        internal void Initialize(NotificationObject obj)
        {
            this.Object = obj;
            this.InitializeInner();
        }

        /// <summary>
        /// 初始化的实现
        /// </summary>
        protected virtual void InitializeInner()
        {

        }

        private AttributeDictionary _attribute;

        private DependencyDelegates _dependencyFunction;

        private DependencyObjects _dependencyObject;

        /// <summary>
        ///     属性字典
        /// </summary>
        [DataMember]
        public AttributeDictionary Attribute
        {
            get
            {
                return this._attribute ?? (this._attribute = new AttributeDictionary());
            }
            set
            {
                this._attribute = value;
            }
        }

        /// <summary>
        ///     依赖方法字典
        /// </summary>
        [IgnoreDataMember]
        public DependencyDelegates DependencyDelegates
        {
            get
            {
                return this._dependencyFunction ?? (this._dependencyFunction = new DependencyDelegates());
            }
            set
            {
                this._dependencyFunction = value;
            }
        }

        /// <summary>
        ///     依赖对象字典
        /// </summary>
        [IgnoreDataMember]
        public DependencyObjects DependencyObjects
        {
            get
            {
                return this._dependencyObject ?? (this._dependencyObject = new DependencyObjects());
            }
            set
            {
                this._dependencyObject = value;
            }
        }
        private IFunctionDictionary _modelFunction;

        /// <summary>
        ///     依赖对象字典
        /// </summary>
        [IgnoreDataMember]
        public IFunctionDictionary ModelFunction
        {
            get
            {
                return this._modelFunction ?? (this._modelFunction = new ModelFunctionDictionary<EntityObjectBase>());
            }
            set
            {
                this._modelFunction = value;
            }
        }
    }
}