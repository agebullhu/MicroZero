namespace ZeroMQ
{
    using lib;
    /// <summary>
    /// ZMQ错误信息
    /// </summary>
    public class ZError : ZSymbol
    {
        static ZError()
        {
            var one = ZSymbol.None;
        }

#pragma warning disable CS1591 // 缺少对公共可见类型或成员的 XML 注释
        public static class Code
#pragma warning restore CS1591 // 缺少对公共可见类型或成员的 XML 注释
        {
            static Code()
            {
                Platform.SetupImplementation(typeof(Code));
            }

            #region ZMQ自定义

            private const int HAUSNUMERO = 156384712;

            // ENOTSUP
#pragma warning disable CS1591 // 缺少对公共可见类型或成员的 XML 注释
            public const int ENOTSUP = HAUSNUMERO + 1;
#pragma warning restore CS1591 // 缺少对公共可见类型或成员的 XML 注释

            // EPROTONOSUPPORT
#pragma warning disable CS1591 // 缺少对公共可见类型或成员的 XML 注释
            public const int EPROTONOSUPPORT = HAUSNUMERO + 2;
#pragma warning restore CS1591 // 缺少对公共可见类型或成员的 XML 注释

            // ENOBUFS
#pragma warning disable CS1591 // 缺少对公共可见类型或成员的 XML 注释
            public const int ENOBUFS = HAUSNUMERO + 3;
#pragma warning restore CS1591 // 缺少对公共可见类型或成员的 XML 注释

            // ENETDOWN
#pragma warning disable CS1591 // 缺少对公共可见类型或成员的 XML 注释
            public const int ENETDOWN = HAUSNUMERO + 4;
#pragma warning restore CS1591 // 缺少对公共可见类型或成员的 XML 注释

            // EADDRINUSE
#pragma warning disable CS1591 // 缺少对公共可见类型或成员的 XML 注释
            public const int EADDRINUSE = HAUSNUMERO + 5;
#pragma warning restore CS1591 // 缺少对公共可见类型或成员的 XML 注释

            // EADDRNOTAVAIL
#pragma warning disable CS1591 // 缺少对公共可见类型或成员的 XML 注释
            public const int EADDRNOTAVAIL = HAUSNUMERO + 6;
#pragma warning restore CS1591 // 缺少对公共可见类型或成员的 XML 注释

            // ECONNREFUSED
#pragma warning disable CS1591 // 缺少对公共可见类型或成员的 XML 注释
            public const int ECONNREFUSED = HAUSNUMERO + 7;
#pragma warning restore CS1591 // 缺少对公共可见类型或成员的 XML 注释

            // EINPROGRESS
#pragma warning disable CS1591 // 缺少对公共可见类型或成员的 XML 注释
            public const int EINPROGRESS = HAUSNUMERO + 8;
#pragma warning restore CS1591 // 缺少对公共可见类型或成员的 XML 注释

            // ENOTSOCK
#pragma warning disable CS1591 // 缺少对公共可见类型或成员的 XML 注释
            public const int ENOTSOCK = HAUSNUMERO + 9;
#pragma warning restore CS1591 // 缺少对公共可见类型或成员的 XML 注释

            // EMSGSIZE
#pragma warning disable CS1591 // 缺少对公共可见类型或成员的 XML 注释
            public const int EMSGSIZE = HAUSNUMERO + 10;
#pragma warning restore CS1591 // 缺少对公共可见类型或成员的 XML 注释

            // EAFNOSUPPORT
#pragma warning disable CS1591 // 缺少对公共可见类型或成员的 XML 注释
            public const int EAFNOSUPPORT = HAUSNUMERO + 11;
#pragma warning restore CS1591 // 缺少对公共可见类型或成员的 XML 注释

            // ENETUNREACH
#pragma warning disable CS1591 // 缺少对公共可见类型或成员的 XML 注释
            public const int ENETUNREACH = HAUSNUMERO + 12;
#pragma warning restore CS1591 // 缺少对公共可见类型或成员的 XML 注释

            // ECONNABORTED
#pragma warning disable CS1591 // 缺少对公共可见类型或成员的 XML 注释
            public const int ECONNABORTED = HAUSNUMERO + 13;
#pragma warning restore CS1591 // 缺少对公共可见类型或成员的 XML 注释

            // ECONNRESET
#pragma warning disable CS1591 // 缺少对公共可见类型或成员的 XML 注释
            public const int ECONNRESET = HAUSNUMERO + 14;
#pragma warning restore CS1591 // 缺少对公共可见类型或成员的 XML 注释

            // ENOTCONN
#pragma warning disable CS1591 // 缺少对公共可见类型或成员的 XML 注释
            public const int ENOTCONN = HAUSNUMERO + 15;
#pragma warning restore CS1591 // 缺少对公共可见类型或成员的 XML 注释

            // ETIMEDOUT
#pragma warning disable CS1591 // 缺少对公共可见类型或成员的 XML 注释
            public const int ETIMEDOUT = HAUSNUMERO + 16;
#pragma warning restore CS1591 // 缺少对公共可见类型或成员的 XML 注释

            // EHOSTUNREACH
#pragma warning disable CS1591 // 缺少对公共可见类型或成员的 XML 注释
            public const int EHOSTUNREACH = HAUSNUMERO + 17;
#pragma warning restore CS1591 // 缺少对公共可见类型或成员的 XML 注释

            // ENETRESET
#pragma warning disable CS1591 // 缺少对公共可见类型或成员的 XML 注释
            public const int ENETRESET = HAUSNUMERO + 18;
#pragma warning restore CS1591 // 缺少对公共可见类型或成员的 XML 注释


            /*  Native 0MQ error codes.*/
#pragma warning disable CS1591 // 缺少对公共可见类型或成员的 XML 注释
            public const int EFSM = HAUSNUMERO + 51;
#pragma warning restore CS1591 // 缺少对公共可见类型或成员的 XML 注释
#pragma warning disable CS1591 // 缺少对公共可见类型或成员的 XML 注释
            public const int ENOCOMPATPROTO = HAUSNUMERO + 52;
#pragma warning restore CS1591 // 缺少对公共可见类型或成员的 XML 注释
#pragma warning disable CS1591 // 缺少对公共可见类型或成员的 XML 注释
            public const int ETERM = HAUSNUMERO + 53;
#pragma warning restore CS1591 // 缺少对公共可见类型或成员的 XML 注释
#pragma warning disable CS1591 // 缺少对公共可见类型或成员的 XML 注释
            public const int EMTHREAD = HAUSNUMERO + 54;
#pragma warning restore CS1591 // 缺少对公共可见类型或成员的 XML 注释


            // NOSUPPORT(AGEBULL)
#pragma warning disable CS1591 // 缺少对公共可见类型或成员的 XML 注释
            public const int NOSUPPORT = HAUSNUMERO + 255;
#pragma warning restore CS1591 // 缺少对公共可见类型或成员的 XML 注释
            #endregion

            #region Windows && Linux
            public const int
#pragma warning disable CS1591 // 缺少对公共可见类型或成员的 XML 注释
                EPERM = 1,
#pragma warning restore CS1591 // 缺少对公共可见类型或成员的 XML 注释
#pragma warning disable CS1591 // 缺少对公共可见类型或成员的 XML 注释
                ENOENT = 2,
#pragma warning restore CS1591 // 缺少对公共可见类型或成员的 XML 注释
#pragma warning disable CS1591 // 缺少对公共可见类型或成员的 XML 注释
                ESRCH = 3,
#pragma warning restore CS1591 // 缺少对公共可见类型或成员的 XML 注释
#pragma warning disable CS1591 // 缺少对公共可见类型或成员的 XML 注释
                EINTR = 4,
#pragma warning restore CS1591 // 缺少对公共可见类型或成员的 XML 注释
#pragma warning disable CS1591 // 缺少对公共可见类型或成员的 XML 注释
                EIO = 5,
#pragma warning restore CS1591 // 缺少对公共可见类型或成员的 XML 注释
#pragma warning disable CS1591 // 缺少对公共可见类型或成员的 XML 注释
                ENXIO = 6,
#pragma warning restore CS1591 // 缺少对公共可见类型或成员的 XML 注释
#pragma warning disable CS1591 // 缺少对公共可见类型或成员的 XML 注释
                E2BIG = 7,
#pragma warning restore CS1591 // 缺少对公共可见类型或成员的 XML 注释
#pragma warning disable CS1591 // 缺少对公共可见类型或成员的 XML 注释
                ENOEXEC = 8,
#pragma warning restore CS1591 // 缺少对公共可见类型或成员的 XML 注释
#pragma warning disable CS1591 // 缺少对公共可见类型或成员的 XML 注释
                EBADF = 9,
#pragma warning restore CS1591 // 缺少对公共可见类型或成员的 XML 注释
#pragma warning disable CS1591 // 缺少对公共可见类型或成员的 XML 注释
                ECHILD = 10,
#pragma warning restore CS1591 // 缺少对公共可见类型或成员的 XML 注释
#pragma warning disable CS1591 // 缺少对公共可见类型或成员的 XML 注释
                EAGAIN = 11,
#pragma warning restore CS1591 // 缺少对公共可见类型或成员的 XML 注释
#pragma warning disable CS1591 // 缺少对公共可见类型或成员的 XML 注释
                ENOMEM = 12,
#pragma warning restore CS1591 // 缺少对公共可见类型或成员的 XML 注释
#pragma warning disable CS1591 // 缺少对公共可见类型或成员的 XML 注释
                EACCES = 13,
#pragma warning restore CS1591 // 缺少对公共可见类型或成员的 XML 注释
#pragma warning disable CS1591 // 缺少对公共可见类型或成员的 XML 注释
                EFAULT = 14,
#pragma warning restore CS1591 // 缺少对公共可见类型或成员的 XML 注释
#pragma warning disable CS1591 // 缺少对公共可见类型或成员的 XML 注释
                ENOTBLK = 15,
#pragma warning restore CS1591 // 缺少对公共可见类型或成员的 XML 注释
#pragma warning disable CS1591 // 缺少对公共可见类型或成员的 XML 注释
                EBUSY = 16,
#pragma warning restore CS1591 // 缺少对公共可见类型或成员的 XML 注释
#pragma warning disable CS1591 // 缺少对公共可见类型或成员的 XML 注释
                EEXIST = 17,
#pragma warning restore CS1591 // 缺少对公共可见类型或成员的 XML 注释
#pragma warning disable CS1591 // 缺少对公共可见类型或成员的 XML 注释
                EXDEV = 18,
#pragma warning restore CS1591 // 缺少对公共可见类型或成员的 XML 注释
#pragma warning disable CS1591 // 缺少对公共可见类型或成员的 XML 注释
                ENODEV = 19,
#pragma warning restore CS1591 // 缺少对公共可见类型或成员的 XML 注释
#pragma warning disable CS1591 // 缺少对公共可见类型或成员的 XML 注释
                ENOTDIR = 20,
#pragma warning restore CS1591 // 缺少对公共可见类型或成员的 XML 注释
#pragma warning disable CS1591 // 缺少对公共可见类型或成员的 XML 注释
                EISDIR = 21,
#pragma warning restore CS1591 // 缺少对公共可见类型或成员的 XML 注释
#pragma warning disable CS1591 // 缺少对公共可见类型或成员的 XML 注释
                EINVAL = 22,
#pragma warning restore CS1591 // 缺少对公共可见类型或成员的 XML 注释
#pragma warning disable CS1591 // 缺少对公共可见类型或成员的 XML 注释
                ENFILE = 23,
#pragma warning restore CS1591 // 缺少对公共可见类型或成员的 XML 注释
#pragma warning disable CS1591 // 缺少对公共可见类型或成员的 XML 注释
                EMFILE = 24,
#pragma warning restore CS1591 // 缺少对公共可见类型或成员的 XML 注释
#pragma warning disable CS1591 // 缺少对公共可见类型或成员的 XML 注释
                ENOTTY = 25,
#pragma warning restore CS1591 // 缺少对公共可见类型或成员的 XML 注释
#pragma warning disable CS1591 // 缺少对公共可见类型或成员的 XML 注释
                ETXTBSY = 26,
#pragma warning restore CS1591 // 缺少对公共可见类型或成员的 XML 注释
#pragma warning disable CS1591 // 缺少对公共可见类型或成员的 XML 注释
                EFBIG = 27,
#pragma warning restore CS1591 // 缺少对公共可见类型或成员的 XML 注释
#pragma warning disable CS1591 // 缺少对公共可见类型或成员的 XML 注释
                ENOSPC = 28,
#pragma warning restore CS1591 // 缺少对公共可见类型或成员的 XML 注释
#pragma warning disable CS1591 // 缺少对公共可见类型或成员的 XML 注释
                ESPIPE = 29,
#pragma warning restore CS1591 // 缺少对公共可见类型或成员的 XML 注释
#pragma warning disable CS1591 // 缺少对公共可见类型或成员的 XML 注释
                EROFS = 30,
#pragma warning restore CS1591 // 缺少对公共可见类型或成员的 XML 注释
#pragma warning disable CS1591 // 缺少对公共可见类型或成员的 XML 注释
                EMLINK = 31,
#pragma warning restore CS1591 // 缺少对公共可见类型或成员的 XML 注释
#pragma warning disable CS1591 // 缺少对公共可见类型或成员的 XML 注释
                EPIPE = 32,
#pragma warning restore CS1591 // 缺少对公共可见类型或成员的 XML 注释
#pragma warning disable CS1591 // 缺少对公共可见类型或成员的 XML 注释
                EDOM = 33,
#pragma warning restore CS1591 // 缺少对公共可见类型或成员的 XML 注释
#pragma warning disable CS1591 // 缺少对公共可见类型或成员的 XML 注释
                ERANGE = 34;
#pragma warning restore CS1591 // 缺少对公共可见类型或成员的 XML 注释

            #endregion

            #region Posix
            internal static class Posix
            {
                // source: http://www.virtsync.com/c-error-codes-include-errno

                public const int
                    // ENOTSUP = HAUSNUMERO + 1,
                    EPROTONOSUPPORT = 93,
                    ENOBUFS = 105,
                    ENETDOWN = 100,
                    EADDRINUSE = 98,
                    EADDRNOTAVAIL = 99,
                    ECONNREFUSED = 111,
                    EINPROGRESS = 115,
                    ENOTSOCK = 88,
                    EMSGSIZE = 90,
                    EAFNOSUPPORT = 97,
                    ENETUNREACH = 101,
                    ECONNABORTED = 103,
                    ECONNRESET = 104,
                    ENOTCONN = 107,
                    ETIMEDOUT = 110,
                    EHOSTUNREACH = 113,
                    ENETRESET = 102
                    ;
            }
            #endregion

            #region MacOSX
            internal static class MacOSX
            {
                public const int
                    EAGAIN = 35,
                    EINPROGRESS = 36,
                    ENOTSOCK = 38,
                    EMSGSIZE = 40,
                    EPROTONOSUPPORT = 43,
                    EAFNOSUPPORT = 47,
                    EADDRINUSE = 48,
                    EADDRNOTAVAIL = 49,
                    ENETDOWN = 50,
                    ENETUNREACH = 51,
                    ENETRESET = 52,
                    ECONNABORTED = 53,
                    ECONNRESET = 54,
                    ENOBUFS = 55,
                    ENOTCONN = 57,
                    ETIMEDOUT = 60,
                    EHOSTUNREACH = 65;
            }
            #endregion
        }
        /// <summary>
        /// 取最后错误
        /// </summary>
        /// <returns></returns>
        public static ZError GetLastErr()
        {
            return new ZError(zmq.errno());
        }

        internal ZError(int errno)
            : base(errno)
        { }
        /// <summary>
        /// 空
        /// </summary>
        public new static ZError None => null;

    }
    /// <summary>
    /// 扩展方便
    /// </summary>
    public static class ZErrorEx
    {
        /// <summary>
        /// 判断是否此错误
        /// </summary>
        /// <param name="err"></param>
        /// <param name="code"></param>
        /// <returns></returns>
        public static bool IsError(this ZError err, int code)
        {
            return err != null && err.Number == code;
        }
    }
}