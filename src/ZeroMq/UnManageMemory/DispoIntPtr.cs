using System.Text;

namespace ZeroMQ.lib
{
    using System;
    using System.Runtime.InteropServices;
    /// <summary>
    /// 非托管内存的管理类
    /// </summary>
    internal sealed partial class DispoIntPtr : MemoryCheck
    {
#if UNMANAGE_MONEY_CHECK
        protected override string TypeName => nameof(DispoIntPtr);
#endif
        public IntPtr Ptr { get; private set; }

        /// <summary>
        /// 自管理构造
        /// </summary>
        private DispoIntPtr(IntPtr ptr)
        {
            Ptr = ptr;
        }

        public static DispoIntPtr Alloc(int size)
        {
            return new DispoIntPtr(Marshal.AllocHGlobal(size));
        }

        public static DispoIntPtr AllocString(string str)
        {
            return new DispoIntPtr(Marshal.StringToHGlobalAnsi(str));
        }

        public static DispoIntPtr AllocString(string str, out int byteCount)
        {
            byteCount = Encoding.ASCII.GetByteCount(str);
            return new DispoIntPtr(Marshal.StringToHGlobalAnsi(str));
        }
        /// <inheritdoc />
        ~DispoIntPtr() => Dispose();

        /// <inheritdoc />
        protected override void DoDispose()
        {
            Marshal.FreeHGlobal(Ptr);
            Ptr = IntPtr.Zero;
        }


        public static implicit operator IntPtr(DispoIntPtr dispoIntPtr)
        {

            return dispoIntPtr == null ? IntPtr.Zero : dispoIntPtr.Ptr;
        }

        public static unsafe explicit operator void* (DispoIntPtr dispoIntPtr)
        {
            return dispoIntPtr == null ? null : (void*)dispoIntPtr.Ptr;
        }

        public static unsafe explicit operator byte* (DispoIntPtr dispoIntPtr)
        {
            return dispoIntPtr == null ? null : (byte*)dispoIntPtr.Ptr;
        }

        public static unsafe explicit operator sbyte* (DispoIntPtr dispoIntPtr)
        {
            return dispoIntPtr == null ? null : (sbyte*)dispoIntPtr.Ptr;
        }

        public static unsafe explicit operator short* (DispoIntPtr dispoIntPtr)
        {
            return dispoIntPtr == null ? null : (short*)dispoIntPtr.Ptr;
        }

        public static unsafe explicit operator ushort* (DispoIntPtr dispoIntPtr)
        {
            return dispoIntPtr == null ? null : (ushort*)dispoIntPtr.Ptr;
        }

        public static unsafe explicit operator char* (DispoIntPtr dispoIntPtr)
        {
            return dispoIntPtr == null ? null : (char*)dispoIntPtr.Ptr;
        }

        public static unsafe explicit operator int* (DispoIntPtr dispoIntPtr)
        {
            return dispoIntPtr == null ? null : (int*)dispoIntPtr.Ptr;
        }

        public static unsafe explicit operator uint* (DispoIntPtr dispoIntPtr)
        {
            return dispoIntPtr == null ? null : (uint*)dispoIntPtr.Ptr;
        }

        public static unsafe explicit operator long* (DispoIntPtr dispoIntPtr)
        {
            return dispoIntPtr == null ? null : (long*)dispoIntPtr.Ptr;
        }

        public static unsafe explicit operator ulong* (DispoIntPtr dispoIntPtr)
        {
            return dispoIntPtr == null ? null : (ulong*)dispoIntPtr.Ptr;
        }

    }
}