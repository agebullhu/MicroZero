using System.Text;

#pragma warning disable CS1591 // 缺少对公共可见类型或成员的 XML 注释
namespace ZeroMQ.lib
{
    using System;
    using System.Runtime.InteropServices;
    /// <summary>
    /// 非托管内存的管理类
    /// </summary>
    public sealed class MarshalPtr : MemoryCheck
    {
#if !UNMANAGE_MONEY_CHECK
        protected override string TypeName { get; }= nameof(MarshalPtr);
#endif

        public IntPtr Ptr { get; private set; }


        /// <summary>
        /// 自管理构造
        /// </summary>
        private MarshalPtr(IntPtr ptr)
        {
            Ptr = ptr;
        }


        public static MarshalPtr Alloc(int size)

        {
            return new MarshalPtr(Marshal.AllocHGlobal(size));
        }


        public static MarshalPtr AllocString(string str)

        {
            return new MarshalPtr(Marshal.StringToHGlobalAnsi(str));
        }


        public static MarshalPtr AllocString(string str, out int byteCount)

        {
            byteCount = Encoding.ASCII.GetByteCount(str);
            return new MarshalPtr(Marshal.StringToHGlobalAnsi(str));
        }
        /// <inheritdoc />
        ~MarshalPtr() => Dispose();

        /// <inheritdoc />
        protected override void DoDispose()
        {
            Marshal.FreeHGlobal(Ptr);
            Ptr = IntPtr.Zero;
        }



        public static implicit operator IntPtr(MarshalPtr dispoIntPtr)

        {

            return dispoIntPtr?.Ptr ?? IntPtr.Zero;
        }


        public static unsafe explicit operator void* (MarshalPtr dispoIntPtr)

        {
            return dispoIntPtr == null ? null : (void*)dispoIntPtr.Ptr;
        }


        public static unsafe explicit operator byte* (MarshalPtr dispoIntPtr)

        {
            return dispoIntPtr == null ? null : (byte*)dispoIntPtr.Ptr;
        }


        public static unsafe explicit operator sbyte* (MarshalPtr dispoIntPtr)

        {
            return dispoIntPtr == null ? null : (sbyte*)dispoIntPtr.Ptr;
        }


        public static unsafe explicit operator short* (MarshalPtr dispoIntPtr)

        {
            return dispoIntPtr == null ? null : (short*)dispoIntPtr.Ptr;
        }


        public static unsafe explicit operator ushort* (MarshalPtr dispoIntPtr)

        {
            return dispoIntPtr == null ? null : (ushort*)dispoIntPtr.Ptr;
        }


        public static unsafe explicit operator char* (MarshalPtr dispoIntPtr)

        {
            return dispoIntPtr == null ? null : (char*)dispoIntPtr.Ptr;
        }


        public static unsafe explicit operator int* (MarshalPtr dispoIntPtr)

        {
            return dispoIntPtr == null ? null : (int*)dispoIntPtr.Ptr;
        }


        public static unsafe explicit operator uint* (MarshalPtr dispoIntPtr)

        {
            return dispoIntPtr == null ? null : (uint*)dispoIntPtr.Ptr;
        }


        public static unsafe explicit operator long* (MarshalPtr dispoIntPtr)

        {
            return dispoIntPtr == null ? null : (long*)dispoIntPtr.Ptr;
        }


        public static unsafe explicit operator ulong* (MarshalPtr dispoIntPtr)

        {
            return dispoIntPtr == null ? null : (ulong*)dispoIntPtr.Ptr;
        }

    }
}