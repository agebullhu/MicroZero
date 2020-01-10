using System;
using System.Collections.Concurrent;
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
            DoDispose();
#if UNMANAGE_MONEY_CHECK
            SetIsFree(TypeName);
            GC.SuppressFinalize(this);
#endif
        }
        /// <summary>
        /// 析构
        /// </summary>
        protected abstract void DoDispose();
#if UNMANAGE_MONEY_CHECK
        
        /// <summary>
        /// 构造
        /// </summary>
        protected MemoryCheck()
        {
            SetIsAloc(TypeName);
        }

        /// <summary>
        /// 显示
        /// </summary>
        public static void Trace()
        {
            Console.WriteLine($"MemoryCheck:{_aliveCount}");
            foreach (var aloc in Alocs.ToArray())
            {
                Console.WriteLine($"MemoryCheck:{aloc.Key}:{aloc.Value}");
            }
        }

        /// <summary>
        /// 分配记录
        /// </summary>
        public static ConcurrentDictionary<string, int> Alocs = new ConcurrentDictionary<string, int>();
        
        /// <summary>
        /// 存活量
        /// </summary>
        public static int AliveCount => _aliveCount;

        /// <summary>
        /// 存活量
        /// </summary>
        private static int _aliveCount;

        /// <summary>
        /// 类型名称
        /// </summary>
        protected abstract string TypeName { get; }

        /// <summary>
        /// 析构
        /// </summary>
        public static void SetIsAloc(string name)
        {
            if (Alocs.ContainsKey(name))
            {
                Alocs[name] += 1;
            }
            else
            {
                Alocs.TryAdd(name, 1);
            }

            _aliveCount++;
        }

        /// <summary>
        /// 析构
        /// </summary>
        public static void SetIsFree(string name)
        {
            Alocs[name] -= 1;
            _aliveCount--;
        }

        /// <summary>
        /// 析构
        /// </summary>
        public void SetIsFree()
        {
            if (_isDisposed > 0)
                return;
            _isDisposed = 1;
            SetIsFree(TypeName);
        }
#endif
    }
}