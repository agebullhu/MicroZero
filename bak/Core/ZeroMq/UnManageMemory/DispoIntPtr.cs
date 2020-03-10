using System.Text;

namespace ZeroMQ.lib
{
    using System;
    using System.Runtime.InteropServices;
    /// <summary>
    /// 非托管内存的管理类
    /// </summary>
    public sealed class DispoIntPtr : MemoryCheck
    {
#if UNMANAGE_MONEY_CHECK
        protected override string TypeName => nameof(DispoIntPtr);
#endif
#pragma warning disable CS1591 // 缺少对公共可见类型或成员的 XML 注释
        public IntPtr Ptr { get; private set; }
#pragma warning restore CS1591 // 缺少对公共可见类型或成员的 XML 注释

        /// <summary>
        /// 自管理构造
        /// </summary>
        private DispoIntPtr(IntPtr ptr)
        {
            Ptr = ptr;
        }

#pragma warning disable CS1591 // 缺少对公共可见类型或成员的 XML 注释
        public static DispoIntPtr Alloc(int size)
#pragma warning restore CS1591 // 缺少对公共可见类型或成员的 XML 注释
        {
            return new DispoIntPtr(Marshal.AllocHGlobal(size));
        }

#pragma warning disable CS1591 // 缺少对公共可见类型或成员的 XML 注释
        public static DispoIntPtr AllocString(string str)
#pragma warning restore CS1591 // 缺少对公共可见类型或成员的 XML 注释
        {
            return new DispoIntPtr(Marshal.StringToHGlobalAnsi(str));
        }

#pragma warning disable CS1591 // 缺少对公共可见类型或成员的 XML 注释
        public static DispoIntPtr AllocString(string str, out int byteCount)
#pragma warning restore CS1591 // 缺少对公共可见类型或成员的 XML 注释
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


#pragma warning disable CS1591 // 缺少对公共可见类型或成员的 XML 注释
        public static implicit operator IntPtr(DispoIntPtr dispoIntPtr)
#pragma warning restore CS1591 // 缺少对公共可见类型或成员的 XML 注释
        {

            return dispoIntPtr == null ? IntPtr.Zero : dispoIntPtr.Ptr;
        }

#pragma warning disable CS1591 // 缺少对公共可见类型或成员的 XML 注释
        public static unsafe explicit operator void* (DispoIntPtr dispoIntPtr)
#pragma warning restore CS1591 // 缺少对公共可见类型或成员的 XML 注释
        {
            return dispoIntPtr == null ? null : (void*)dispoIntPtr.Ptr;
        }

#pragma warning disable CS1591 // 缺少对公共可见类型或成员的 XML 注释
        public static unsafe explicit operator byte* (DispoIntPtr dispoIntPtr)
#pragma warning restore CS1591 // 缺少对公共可见类型或成员的 XML 注释
        {
            return dispoIntPtr == null ? null : (byte*)dispoIntPtr.Ptr;
        }

#pragma warning disable CS1591 // 缺少对公共可见类型或成员的 XML 注释
        public static unsafe explicit operator sbyte* (DispoIntPtr dispoIntPtr)
#pragma warning restore CS1591 // 缺少对公共可见类型或成员的 XML 注释
        {
            return dispoIntPtr == null ? null : (sbyte*)dispoIntPtr.Ptr;
        }

#pragma warning disable CS1591 // 缺少对公共可见类型或成员的 XML 注释
        public static unsafe explicit operator short* (DispoIntPtr dispoIntPtr)
#pragma warning restore CS1591 // 缺少对公共可见类型或成员的 XML 注释
        {
            return dispoIntPtr == null ? null : (short*)dispoIntPtr.Ptr;
        }

#pragma warning disable CS1591 // 缺少对公共可见类型或成员的 XML 注释
        public static unsafe explicit operator ushort* (DispoIntPtr dispoIntPtr)
#pragma warning restore CS1591 // 缺少对公共可见类型或成员的 XML 注释
        {
            return dispoIntPtr == null ? null : (ushort*)dispoIntPtr.Ptr;
        }

#pragma warning disable CS1591 // 缺少对公共可见类型或成员的 XML 注释
        public static unsafe explicit operator char* (DispoIntPtr dispoIntPtr)
#pragma warning restore CS1591 // 缺少对公共可见类型或成员的 XML 注释
        {
            return dispoIntPtr == null ? null : (char*)dispoIntPtr.Ptr;
        }

#pragma warning disable CS1591 // 缺少对公共可见类型或成员的 XML 注释
        public static unsafe explicit operator int* (DispoIntPtr dispoIntPtr)
#pragma warning restore CS1591 // 缺少对公共可见类型或成员的 XML 注释
        {
            return dispoIntPtr == null ? null : (int*)dispoIntPtr.Ptr;
        }

#pragma warning disable CS1591 // 缺少对公共可见类型或成员的 XML 注释
        public static unsafe explicit operator uint* (DispoIntPtr dispoIntPtr)
#pragma warning restore CS1591 // 缺少对公共可见类型或成员的 XML 注释
        {
            return dispoIntPtr == null ? null : (uint*)dispoIntPtr.Ptr;
        }

#pragma warning disable CS1591 // 缺少对公共可见类型或成员的 XML 注释
        public static unsafe explicit operator long* (DispoIntPtr dispoIntPtr)
#pragma warning restore CS1591 // 缺少对公共可见类型或成员的 XML 注释
        {
            return dispoIntPtr == null ? null : (long*)dispoIntPtr.Ptr;
        }

#pragma warning disable CS1591 // 缺少对公共可见类型或成员的 XML 注释
        public static unsafe explicit operator ulong* (DispoIntPtr dispoIntPtr)
#pragma warning restore CS1591 // 缺少对公共可见类型或成员的 XML 注释
        {
            return dispoIntPtr == null ? null : (ulong*)dispoIntPtr.Ptr;
        }

    }
}