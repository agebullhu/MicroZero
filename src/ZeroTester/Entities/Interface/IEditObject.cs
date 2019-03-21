// /*****************************************************
// (c)2008-2013 Copy right www.Agebull.com
// 作者:bull2
// 工程:CodeRefactor-Agebull.Common.WpfMvvmBase
// 建立:2014-11-29
// 修改:2014-11-29
// *****************************************************/

#region 引用

using System.ComponentModel;

#endregion

namespace Agebull.EntityModel
{
    public interface IEntityObject
    {
        /// <summary>
        /// 复制值
        /// </summary>
        /// <param name="source">复制的源字段</param>
        void CopyValue(IEntityObject source);

        /// <summary>
        ///     得到字段的值
        /// </summary>
        /// <param name="field"> 字段的名字 </param>
        /// <returns> 字段的值 </returns>
        object GetValue(string field);

        /// <summary>
        ///     配置字段的值
        /// </summary>
        /// <param name="field"> 字段的名字 </param>
        /// <param name="value"> 字段的值 </param>
        void SetValue(string field, object value);
    }
    /// <summary>
    ///     编辑对象
    /// </summary>
    public interface IEditObject : IEntityObject
#if CLIENT
        , INotifyPropertyChanging, INotifyPropertyChanged
#endif
    {
        /// <summary>
        ///     是否修改
        /// </summary>
        bool IsModified
        {
            get;
        }

        /// <summary>
        ///     是否已删除
        /// </summary>
        bool IsDelete
        {
            get;
        }

        /// <summary>
        ///     是否新增
        /// </summary>
        bool IsAdd
        {
            get;
        }

        /// <summary>
        ///     是否修改
        /// </summary>
        /// <param name="field"> 字段的名字 </param>
        bool FieldIsModified(string field);

        /// <summary>
        ///     设置为非改变
        /// </summary>
        /// <param name="field"> 字段的名字 </param>
        void SetUnModify(string field);

        /// <summary>
        ///     设置为改变
        /// </summary>
        /// <param name="field"> 字段的名字 </param>
        void SetModify(string field);

        /// <summary>
        ///     接受修改
        /// </summary>
        void AcceptChanged();

        /// <summary>
        ///     回退修改
        /// </summary>
        void RejectChanged();

        /// <summary>
        /// 属性修改的后期处理(保存后)
        /// </summary>
        /// <remarks>
        /// 对当前对象的属性的更改,请自行保存,否则将丢失
        /// </remarks>
        void LaterPeriodByModify();
    }
}
