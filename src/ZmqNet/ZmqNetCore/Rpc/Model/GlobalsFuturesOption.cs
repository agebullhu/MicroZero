/*design by:agebull designer date:2017/5/13 21:41:08*/

using Agebull.Common.DataModel;

namespace Agebull.Zmq.Rpc
{
    
    /// <summary>
    ///     外盘期货交易系统的全局内容
    /// </summary>
    public class GlobalsFuturesOption
    {
        #region 注册类型
        
        /// <summary>
        /// 注册类型
        /// </summary>
        public static void ReigsterEntityType()
        {
            TsonTypeRegister.RegisteType<StringArgumentData>(StringArgumentData.EntityId, "文本的参数");
        }
        #endregion
    }
}