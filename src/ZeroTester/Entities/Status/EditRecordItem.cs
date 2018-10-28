// /*****************************************************
// (c)2008-2013 Copy right www.Gboxt.com
// 作者:bull2
// 工程:CodeRefactor-Gboxt.Common.WpfMvvmBase
// 建立:2014-11-29
// 修改:2014-11-29
// *****************************************************/

namespace Agebull.Common.DataModel
{
    /// <summary>
    ///     编辑记录节点
    /// </summary>
    public sealed class EditRecordItem
    {
        /// <summary>
        ///     修改前的值
        /// </summary>
        public object Value
        {
            get;
            internal set;
        }

        /// <summary>
        ///     修改的属性
        /// </summary>
        public string Property
        {
            get;
            internal set;
        }
    }
}
