using System;
using System.Threading;
using Agebull.Common.Base;
using Agebull.Common.Logging;

namespace Agebull.MicroZero
{
    /// <summary>
    /// 锁定数据
    /// </summary>
    public sealed class LockData
    {
        /// <summary>
        /// 锁定对象
        /// </summary>
        public int LockObj = 0;
    }

    /// <summary>
    /// 保证独立操作的范围(让启动的Task中使用相同范围时会在在此范围完成后顺序执行)
    /// </summary>
    public class OnceScope : ScopeBase
    {
        /// <summary>
        /// 锁定对象
        /// </summary>
        private readonly LockData _lockObj;

        /// <summary>
        /// 锁定对象
        /// </summary>
        private readonly Action _closeAction;

        /// <summary>
        /// 是否进入
        /// </summary>
        public bool IsEntry { get; }

        /// <summary>
        /// 内部构造
        /// </summary>
        /// <param name="lockObj"></param>
        /// <param name="open"></param>
        /// <param name="close"></param>
        private OnceScope(LockData lockObj, Action open, Action close)
        {
            _lockObj = lockObj;
            _closeAction = close;
            IsEntry = Interlocked.Increment(ref _lockObj.LockObj) == 1;
            try
            {
                if (IsEntry)
                    open?.Invoke();
            }
            catch (Exception e)
            {
                LogRecorderX.Exception(e);
            }
        }

        /// <summary>
        /// 构造范围
        /// </summary>
        /// <param name="lockObj">锁定对象</param>
        /// <param name="open">进入时执行的动作</param>
        /// <param name="close">离开时必须执行的动作</param>
        /// <returns>如果超时，则返回空。正常进入返回对象</returns>
        public static OnceScope TryCreateScope(LockData lockObj, Action open = null, Action close = null)
        {
            return new OnceScope(lockObj, open, close);
        }

        /// <inheritdoc />
        protected override void OnDispose()
        {
            try
            {
                Interlocked.Decrement(ref _lockObj.LockObj);
                if (IsEntry)
                    _closeAction?.Invoke();
            }
            catch (Exception e)
            {
                LogRecorderX.Exception(e);
            }
        }
    }
}