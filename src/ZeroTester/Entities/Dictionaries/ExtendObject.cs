// /*****************************************************
// (c)2008-2013 Copy right www.Agebull.com
// 作者:bull2
// 工程:CodeRefactor-Agebull.Common.WpfMvvmBase
// 建立:2014-11-29
// 修改:2014-11-29
// *****************************************************/

#region 引用

using System.Runtime.Serialization;
using Newtonsoft.Json;

#endregion

namespace Agebull.EntityModel
{
    /// <summary>
    /// 表示有扩展属性的对象
    /// </summary>
    public interface IExtendAttribute
    {
        /// <summary>
        ///     属性字典
        /// </summary>
        AttributeDictionary Attribute
        {
            get;
        }
    }
    /// <summary>
    /// 表示有依赖属性的对象
    /// </summary>
    public interface IExtendDependencyObjects
    {
        /// <summary>
        ///     依赖字典
        /// </summary>
        DependencyObjects Dependency
        {
            get;
        }
    }
    /// <summary>
    /// 表示有扩展方法(对象类型访问)的对象
    /// </summary>
    public interface IExtendDependencyDelegates
    {
        /// <summary>
        ///     扩展方法字典
        /// </summary>
        DependencyDelegates Delegates
        {
            get;
        }
    }
    /// <summary>
    /// 表示有扩展方法(名称访问)的对象
    /// </summary>
    public interface IExtendDelegates
    {
        /// <summary>
        ///     扩展方法字典
        /// </summary>
        IFunctionDictionary Delegates
        {
            get;
        }
    }
    /// <summary>
    /// 表示有扩展方法(名称访问)的对象
    /// </summary>
    public interface IExtendModelDelegates<TModel> where TModel : class
    {

        /// <summary>
        ///     依赖对象字典
        /// </summary>
        ModelFunctionDictionary<TModel> ModelFunction
        {
            get;
        }
    }
    /// <summary>
    ///     扩展对象
    /// </summary>
    [DataContract, JsonObject(MemberSerialization.OptIn)]
    public class ExtendObject
    {
        private AttributeDictionary _attribute;

        private DependencyDelegates _dependencyFunction;

        private DependencyObjects _dependencyObject;

        /// <summary>
        ///     属性字典
        /// </summary>
        [DataMember]
        public AttributeDictionary Attribute
        {
            get => _attribute ?? (_attribute = new AttributeDictionary() );
            set => _attribute = value;
        }

        /// <summary>
        ///     依赖方法字典
        /// </summary>
        [IgnoreDataMember]
        public DependencyDelegates DependencyDelegates
        {
            get => _dependencyFunction ?? (_dependencyFunction = new DependencyDelegates() );
            set => _dependencyFunction = value;
        }

        /// <summary>
        ///     依赖对象字典
        /// </summary>
        [IgnoreDataMember]
        public DependencyObjects DependencyObjects
        {
            get => _dependencyObject ?? (_dependencyObject = new DependencyObjects() );
            set => _dependencyObject = value;
        }
    }

    /// <summary>
    ///     扩展对象
    /// </summary>
    [DataContract, JsonObject(MemberSerialization.OptIn)]
    public sealed class ExtendObject<TModel> : ExtendObject
            where TModel : class
    {
        private ModelFunctionDictionary<TModel> _modelFunction;

        /// <summary>
        ///     依赖对象字典
        /// </summary>
        [IgnoreDataMember]
        public ModelFunctionDictionary<TModel> ModelFunction
        {
            get => _modelFunction ?? (_modelFunction = new ModelFunctionDictionary<TModel>() );
            set => _modelFunction = value;
        }
    }
}
