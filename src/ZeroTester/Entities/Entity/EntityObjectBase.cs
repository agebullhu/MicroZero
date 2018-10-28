using System;
using System.Collections.Generic;
using System.Runtime.Serialization;

namespace Agebull.EntityModel
{
    /// <summary>
    ///     实体基类
    /// </summary>
    [DataContract, KnownType("GetKnownTypes")]
    public abstract class EntityObjectBase : NotificationObject, IEntityObject
    {
        #region 序列化类型支持

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
        public static Type[] KnownTypes => _haseNew ? ( _types = typeList.ToArray() ) : _types;

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

        #region 实体操作支持

        /// <summary>
        /// 复制值
        /// </summary>
        /// <param name="source">复制的源字段</param>
        public void CopyValue(IEntityObject source)
        {
            var entity = source as EntityObjectBase;
            if(entity != null)
                CopyValueInner(entity);
        }

        /// <summary>
        ///     设置属性值
        /// </summary>
        /// <param name="property"></param>
        /// <param name="value"></param>
        public void SetValue(string property, object value)
        {
            SetValueInner(property, value);
        }

        /// <summary>
        ///     设置属性值
        /// </summary>
        /// <param name="property"></param>
        public object GetValue(string property)
        {
            return GetValueInner(property);
        }

        /// <summary>
        ///     设置属性值
        /// </summary>
        /// <param name="property"></param>
        public TValue GetValue<TValue>(string property)
        {
            return GetValueInner<TValue>(property);
        }
        #endregion

        #region 内部实现

        /// <summary>
        /// 复制值
        /// </summary>
        /// <param name="source">复制的源字段</param>
        protected abstract void CopyValueInner(EntityObjectBase source);

        /// <summary>
        ///     设置属性值
        /// </summary>
        /// <param name="property"></param>
        /// <param name="value"></param>
        protected abstract void SetValueInner(string property, object value);

        /// <summary>
        ///     设置属性值
        /// </summary>
        /// <param name="property"></param>
        protected abstract object GetValueInner(string property);

        /// <summary>
        ///     设置属性值
        /// </summary>
        /// <param name="property"></param>
        protected virtual TValue GetValueInner<TValue>(string property)
        {
            return (TValue)GetValue(property);
        }
        #endregion

    }
}