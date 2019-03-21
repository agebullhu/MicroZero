// // /*****************************************************
// // (c)2016-2016 Copy right www.gboxt.com
// // 作者:
// // 工程:Agebull.DataModel
// // 建立:2016-06-12
// // 修改:2016-06-24
// // *****************************************************/

#region 引用

using System.ComponentModel;
using System.Runtime.Serialization;
using Agebull.Common.DataModel;
using Agebull.Common.Rpc;
using Agebull.ZeroNet.ZeroApi;
using Newtonsoft.Json;

#endregion

namespace Xuhui.Internetpro.WzHealthCardService
{
    /// <summary>
    ///     为业务处理上下文对象
    /// </summary>
    [DataContract]
    [JsonObject(MemberSerialization.OptIn)]
    public class WzHealthCardContext : GlobalContext
    {
        /// <summary>
        ///     取得或设置线程单例对象，当前对象不存在时，会自动构架一个
        /// </summary>
        public static WzHealthCardContext Context => Current as WzHealthCardContext;

        /// <summary>
        /// 当前参数
        /// </summary>
        private ApiArgument _argument;

        /// <summary>
        /// 当前参数
        /// </summary>
        public ApiArgument Argument
        {
            get
            {
                if (_argument != null)
                    return _argument;
                return _argument = DependencyObjects.Dependency<ApiAction>()?.Argument as ApiArgument;
            }

        }
    }
}