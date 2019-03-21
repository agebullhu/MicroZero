// /*****************************************************
// (c)2008-2013 Copy right www.Agebull.com
// 作者:bull2
// 工程:CodeRefactor-Agebull.Common.WpfMvvmBase
// 建立:2014-11-27
// 修改:2014-12-07
// *****************************************************/

#region 引用

using System.ComponentModel;
using System.Windows;
using System.Windows.Input;

#endregion

namespace Agebull.Common.Mvvm
{
    /// <summary>
    ///     表明提供状态信息的命令
    /// </summary>
    public interface IStatus
    {
        /// <summary>
        ///     是否正忙
        /// </summary>
        bool IsBusy
        {
            get;
        }

        /// <summary>
        ///     当前状态
        /// </summary>
        CommandStatus Status
        {
            get;
        }
        /// <summary>
        ///     图标
        /// </summary>
        Visibility Visibility
        {
            get;
        }
    }
    /// <summary>
    ///     表明提供状态信息的命令
    /// </summary>
    public interface IStatusCommand : ICommand, IStatus
    {
    }

    /// <summary>
    ///     表示一个异步命令
    /// </summary>
    public interface IAsyncCommand : INotifyPropertyChanged, IStatusCommand
    {
    }
}
