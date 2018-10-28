// /*****************************************************
// (c)2008-2013 Copy right www.Gboxt.com
// 作者:bull2
// 工程:CodeRefactor-Gboxt.Common.WpfMvvmBase
// 建立:2014-11-29
// 修改:2014-11-29
// *****************************************************/

#region 引用

using System.Runtime.Serialization;
using Newtonsoft.Json;

#endregion

namespace Agebull.Common.DataModel
{
    /// <summary>
    ///     保存原始值的实体对象
    /// </summary>
    [DataContract, JsonObject(MemberSerialization.OptIn)]
    public abstract class OriginalRecordEntityObject : EditEntityObject<OriginalRecordStatus>
    {
    }
}
