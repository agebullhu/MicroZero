using System;
#if UNMANAGE_MONEY_CHECK
using System.Collections.Generic;
using System.Threading;
#endif

namespace ZeroMQ
{
    /// <summary>
    /// 内存检查对象
    /// </summary>
    public abstract class MemoryCheck : IDisposable
    {
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
#if UNMANAGE_MONEY_CHECK
            SetIsAloc(TypeName);
#endif
        }

        private int _isDisposed;

        /// <summary>
        /// 已析构
        /// </summary>
        public bool IsDisposed => _isDisposed > 0;


        /// <summary>
        /// 析构
        /// </summary>
        public void Dispose()
        {
            if (_isDisposed > 0)
                return;
            _isDisposed = 1;

#if UNMANAGE_MONEY_CHECK
            SetIsFree(TypeName);
#endif
            DoDispose();
            GC.SuppressFinalize(this);
        }
        /// <summary>
        /// 析构
        /// </summary>
        protected abstract void DoDispose();
#if UNMANAGE_MONEY_CHECK
        public static Dictionary<string, int> Alocs = new Dictionary<string, int>();
        /// <summary>
        /// 存活量
        /// </summary>
        public static int AliveCount => _aliveCount;

        /// <summary>
        /// 存活量
        /// </summary>
        private static int _aliveCount;
        protected abstract string TypeName { get; }
/// <summary>
/// 析构
/// </summary>
        public static void SetIsAloc(string name)
        {
            lock (Alocs)
            {
                if (Alocs.ContainsKey(name))
                {
                    Alocs[name] += 1;
                }
                else
                {
                    Alocs.Add(name, 1);
                }
            }
            _aliveCount++;
        }
        /// <summary>
        /// 析构
        /// </summary>
        public static void SetIsFree(string name)
        {
            lock (Alocs)
            {
                Alocs[name] -= 1;
            }
            _aliveCount--;
        }
/// <summary>
/// 析构
/// </summary>
        public void SetIsFree()
        {
            if (_isDisposed > 0)
                return;
            _isDisposed=1;
            SetIsAloc(TypeName);
        }
#endif
    }
}