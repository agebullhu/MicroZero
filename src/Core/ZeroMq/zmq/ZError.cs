using System.Linq;

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

        public static class Code
        {
            static Code()
            {
                Platform.SetupImplementation(typeof(Code));
            }

            #region ZMQ自定义

            private const int HAUSNUMERO = 156384712;

            // ENOTSUP
            public const int ENOTSUP = HAUSNUMERO + 1;

            // EPROTONOSUPPORT
            public const int EPROTONOSUPPORT = HAUSNUMERO + 2;

            // ENOBUFS
            public const int ENOBUFS = HAUSNUMERO + 3;

            // ENETDOWN
            public const int ENETDOWN = HAUSNUMERO + 4;

            // EADDRINUSE
            public const int EADDRINUSE = HAUSNUMERO + 5;

            // EADDRNOTAVAIL
            public const int EADDRNOTAVAIL = HAUSNUMERO + 6;

            // ECONNREFUSED
            public const int ECONNREFUSED = HAUSNUMERO + 7;

            // EINPROGRESS
            public const int EINPROGRESS = HAUSNUMERO + 8;

            // ENOTSOCK
            public const int ENOTSOCK = HAUSNUMERO + 9;

            // EMSGSIZE
            public const int EMSGSIZE = HAUSNUMERO + 10;

            // EAFNOSUPPORT
            public const int EAFNOSUPPORT = HAUSNUMERO + 11;

            // ENETUNREACH
            public const int ENETUNREACH = HAUSNUMERO + 12;

            // ECONNABORTED
            public const int ECONNABORTED = HAUSNUMERO + 13;

            // ECONNRESET
            public const int ECONNRESET = HAUSNUMERO + 14;

            // ENOTCONN
            public const int ENOTCONN = HAUSNUMERO + 15;

            // ETIMEDOUT
            public const int ETIMEDOUT = HAUSNUMERO + 16;

            // EHOSTUNREACH
            public const int EHOSTUNREACH = HAUSNUMERO + 17;

            // ENETRESET
            public const int ENETRESET = HAUSNUMERO + 18;


            /*  Native 0MQ error codes.*/
            public const int EFSM = HAUSNUMERO + 51;
            public const int ENOCOMPATPROTO = HAUSNUMERO + 52;
            public const int ETERM = HAUSNUMERO + 53;
            public const int EMTHREAD = HAUSNUMERO + 54;


            // NOSUPPORT(AGEBULL)
            public const int NOSUPPORT = HAUSNUMERO + 255;
            #endregion

            #region Windows && Linux
            public const int
                EPERM = 1,
                ENOENT = 2,
                ESRCH = 3,
                EINTR = 4,
                EIO = 5,
                ENXIO = 6,
                E2BIG = 7,
                ENOEXEC = 8,
                EBADF = 9,
                ECHILD = 10,
                EAGAIN = 11,
                ENOMEM = 12,
                EACCES = 13,
                EFAULT = 14,
                ENOTBLK = 15,
                EBUSY = 16,
                EEXIST = 17,
                EXDEV = 18,
                ENODEV = 19,
                ENOTDIR = 20,
                EISDIR = 21,
                EINVAL = 22,
                ENFILE = 23,
                EMFILE = 24,
                ENOTTY = 25,
                ETXTBSY = 26,
                EFBIG = 27,
                ENOSPC = 28,
                ESPIPE = 29,
                EROFS = 30,
                EMLINK = 31,
                EPIPE = 32,
                EDOM = 33,
                ERANGE = 34;

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
        public new static ZError None => default(ZError);

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