// /*****************************************************
// (c)2008-2013 Copy right www.Agebull.com
// 作者:bull2
// 工程:CodeRefactor-Agebull.Common.WpfMvvmBase
// 建立:2014-11-26
// 修改:2014-12-07
// *****************************************************/

namespace Agebull.Common.Mvvm
{
    /// <summary>
    ///     命令状态
    /// </summary>
    public enum CommandStatus
    {
        /// <summary>
        ///     未执行
        /// </summary>
        None,

        /// <summary>
        ///     未执行
        /// </summary>
        Disable,

        /// <summary>
        ///     发生异常
        /// </summary>
        Faulted,

        /// <summary>
        ///     执行中
        /// </summary>
        Executing,

        /// <summary>
        ///     执行完成
        /// </summary>
        Succeed
    }
}
