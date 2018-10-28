using System;
using Agebull.Common.Mvvm;

namespace Agebull.EntityModel
{

    /// <summary>
    /// 表示Task表示异步代理对象
    /// </summary>
    public interface ITackProxy
    {
        /// <summary>
        ///     当前状态
        /// </summary>
        CommandStatus Status
        {
            get;
            set;
        }

        /// <summary>
        /// 能否执行
        /// </summary>
        /// <returns>null表示取消任务,treu表示可以立即执行,false表示应该等待前一个任务完成后执行</returns>
        bool? CanDo();

        void Run(Action task);

        void Exist();
    }


    /// <summary>
    /// 表示异步代理对象
    /// </summary>
    public interface ITackProxy<in TResult>
    {
        void Run(Func<TResult> task);

        void Exist(TResult result = default(TResult));
    }
}