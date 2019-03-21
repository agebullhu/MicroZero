using System;
using System.Threading;
using Agebull.Common.Base;
using Agebull.Common.Logging;

namespace Agebull.MicroZero
{
    /// <summary>
    /// 保证独立操作的范围(让启动的Task中使用相同范围时会在在此范围完成后顺序执行)
    /// </summary>
    public class OnceScope : ScopeBase
    {
        /// <summary>
        /// 锁定对象
        /// </summary>
        private readonly object _lockObj;

        /// <summary>
        /// 锁定对象
        /// </summary>
        private readonly Action _closeAction;

        /// <summary>
        /// 内部构造
        /// </summary>
        /// <param name="lockObj"></param>
        /// <param name="open"></param>
        /// <param name="close"></param>
        private OnceScope(object lockObj, Action open, Action close)
        {
            _lockObj = lockObj;
            _closeAction = close;
            try
            {
                open?.Invoke();
            }
            catch (Exception e)
            {
                LogRecorder.Exception(e);
            }
        }

        /// <summary>
        /// 构造范围
        /// </summary>
        /// <param name="lockObj">锁定对象</param>
        /// <param name="open">进入时执行的动作</param>
        /// <param name="close">离开时必须执行的动作</param>
        /// <returns></returns>
        public static OnceScope CreateScope(object lockObj, Action open = null, Action close = null)
        {
            if (!Monitor.TryEnter(lockObj, 30000))
                throw new ArgumentException("参数{lockObj}已锁死（10S）");
            return new OnceScope(lockObj, open, close);
        }

        /// <summary>
        /// 构造范围
        /// </summary>
        /// <param name="lockObj">锁定对象</param>
        /// <param name="timeOut">超时时长</param>
        /// <param name="open">进入时执行的动作</param>
        /// <param name="close">离开时必须执行的动作</param>
        /// <returns>如果超时，则返回空。正常进入返回对象</returns>
        public static OnceScope TryCreateScope(object lockObj,int timeOut, Action open = null, Action close = null)
        {
            return !Monitor.TryEnter(lockObj, timeOut) ? null : new OnceScope(lockObj, open, close);
        }

        /// <inheritdoc />
        protected override void OnDispose()
        {
            Monitor.Exit(_lockObj);
            try
            {
                _closeAction?.Invoke();
            }
            catch (Exception e)
            {
                LogRecorder.Exception(e);
            }
        }
    }
}