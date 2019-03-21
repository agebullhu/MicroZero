// /*****************************************************
// (c)2008-2013 Copy right www.Agebull.com
// 作者:bull2
// 工程:CodeRefactor-Agebull.Common.WpfMvvmBase
// 建立:2014-11-24
// 修改:2014-11-29
// *****************************************************/

#region 引用

using System.Diagnostics;

#endregion

namespace Agebull.EntityModel
{
    /// <summary>
    ///    消息侦测器
    /// </summary>
    public sealed class MessageTraceListener : TraceListener
    {
        public override void Write(string message)
        {
            //if (TraceMessage.DefaultTrace.MessageToTrace)
            {
                TraceMessage.DefaultTrace.Track = message;
            }
        }

        public override void WriteLine(string message)
        {
            //if (TraceMessage.DefaultTrace.MessageToTrace)
            {
                TraceMessage.DefaultTrace.Track = message;
            }
        }
    }
}
