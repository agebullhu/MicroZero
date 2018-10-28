// /*****************************************************
// (c)2008-2013 Copy right www.Gboxt.com
// 作者:bull2
// 工程:CodeRefactor-Gboxt.Common.WpfMvvmBase
// 建立:2014-11-29
// 修改:2014-11-29
// *****************************************************/

#region 引用

using System.Runtime.Serialization;

#endregion

namespace Agebull.Common.DataModel
{
    /// <summary>
    ///     表示网路对象
    /// </summary>
    public interface INetObject
    {
        /// <summary>
        ///     表示网路对象来源
        /// </summary>
        [DataMember(Name = "NetObjectSource")]
        NetObjectSource NetObjectSource
        {
            get;
            set;
        }

        /// <summary>
        ///     序列化中
        /// </summary>
        bool IsSerializing
        {
            get;
            set;
        }

        /// <summary>
        ///     反序列化中
        /// </summary>
        bool IsDeserializing
        {
            get;
            set;
        }
    }
}
