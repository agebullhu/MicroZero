using Gboxt.Common.DataModel;
using System.Collections.Generic;

namespace Agebull.Common.DataModel
{
    /// <summary>
    ///     实体基类
    /// </summary>
    public abstract class EntityBase : NotificationObject
    {
        #region 实体操作

        /// <summary>
        /// 复制值
        /// </summary>
        /// <param name="entity">复制的源字段</param>
        public void CopyValue(EntityBase entity)
        {
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
            property = property?.ToLower();
            SetValueInner(property, value);
        }

        /// <summary>
        ///     设置属性值
        /// </summary>
        /// <param name="property"></param>
        public object GetValue(string property)
        {
            property = property?.ToLower();
            return GetValueInner(property);
        }

        /// <summary>
        ///     设置属性值
        /// </summary>
        /// <param name="property"></param>
        public TValue GetValue<TValue>(string property)
        {
            property = property?.ToLower();
            return GetValueInner<TValue>(property);
        }
        #endregion

        #region 内部实现

        /// <summary>
        /// 复制值
        /// </summary>
        /// <param name="source">复制的源字段</param>
        protected abstract void CopyValueInner(EntityBase source);
        
        /// <summary>
        ///     设置属性值
        /// </summary>
        /// <param name="property"></param>
        protected virtual TValue GetValueInner<TValue>(string property)
        {
            return (TValue)GetValue(property);
        }
        /// <summary>
        ///     设置属性值
        /// </summary>
        /// <param name="property"></param>
        protected abstract object GetValueInner(string property);

        /// <summary>
        ///     设置属性值
        /// </summary>
        /// <param name="property"></param>
        /// <param name="value"></param>
        protected abstract void SetValueInner(string property, object value);

        #endregion

        #region 修改列表

        /// <summary>
        /// 修改列表
        /// </summary>
        private readonly List<string> _modifiedList = new List<string>();

        /// <summary>
        ///     属性修改处理
        /// </summary>
        /// <param name="propertyName">属性名称</param>
        protected override void OnPropertyChangedInner(string propertyName)
        {
            base.OnPropertyChangedInner(propertyName);
            if (!_modifiedList.Contains(propertyName))
                _modifiedList.Add(propertyName);
        }

        #endregion
    }
}