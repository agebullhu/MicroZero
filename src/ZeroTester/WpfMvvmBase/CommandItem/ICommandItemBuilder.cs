using System;
using System.Collections;

namespace Agebull.Common.Mvvm
{
    /// <summary>
    /// 表示一个命令生成器
    /// </summary>
    public interface ICommandItemBuilder : ICommandItem
    {
        /// <summary>
        /// 转为命令对象
        /// </summary>
        /// <returns>命令对象</returns>
        CommandItemBase ToCommand(object arg, Func<object, IEnumerator> enumerator);
    }
}