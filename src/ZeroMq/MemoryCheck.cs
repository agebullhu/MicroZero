using System;
using System.Threading;

namespace ZeroMQ
{
    /// <summary>
    /// 内存检查对象
    /// </summary>
    public abstract class MemoryCheck : IDisposable
    {
        /// <summary>
        /// 存活量
        /// </summary>
        public static int AliveCount => _aliveCount;

        /// <summary>
        /// 存活量
        /// </summary>
        private static int _aliveCount;

        /// <summary>
        /// 析构
        /// </summary>
        ~MemoryCheck()
        {
            Dispose();
        }
        /// <summary>
        /// 构造
        /// </summary>
        protected MemoryCheck()
        {
            _aliveCount++;
        }
        private int _isDisposed;

        /// <summary>
        /// 析构
        /// </summary>
        public void Dispose()
        {
            if (_isDisposed > 0)
                return;
            Interlocked.Increment(ref _isDisposed);
            if (_isDisposed > 1)
                return;
            _aliveCount--;
            DoDispose();
        }
        /// <summary>
        /// 析构
        /// </summary>
        protected abstract void DoDispose();
    }
}