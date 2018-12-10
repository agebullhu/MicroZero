using System;
using System.IO;
using System.Runtime.InteropServices;
using System.Text;

using ZeroMQ.lib;

namespace ZeroMQ
{

#pragma warning disable CS1591 // 缺少对公共可见类型或成员的 XML 注释
    /// <summary>
    /// A single part message, sent or received via a <see cref="ZSocket"/>.
    /// </summary>
    public sealed class ZFrame : Stream, ICloneable
    {
        public static ZFrame FromStream(Stream stream, long i, int l)
        {
            stream.Position = i;
            if (l == 0) return new ZFrame();

            var frame = Create(l);
            var buf = new byte[65535];
            int bufLen;
            int current = -1;
            do
            {
                ++current;
                var remaining = Math.Min(Math.Max(0, l - current * buf.Length), buf.Length);
                if (remaining < 1) break;

                bufLen = stream.Read(buf, 0, remaining);
                frame.Write(buf, 0, bufLen);

            } while (bufLen > 0);

            frame.Position = 0;
            return frame;
        }

        public static ZFrame CopyFrom(ZFrame frame)
        {
            return frame.Duplicate();
        }

        public static ZFrame Create(int size)
        {
            if (size < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(size));
            }
            return new ZFrame(CreateNative(size), size);
        }

        public static ZFrame CreateEmpty()
        {
            return new ZFrame(CreateEmptyNative(), -1);
        }

        public const int DefaultFrameSize = 4096;

        public static readonly int MinimumFrameSize = zmq.sizeof_zmq_msg_t;

        private DispoIntPtr FramePtr
        {
            get => _framePtr;
            set
            {
                if (null == value)
                {
                    _framePtr?.Dispose();
                    _framePtr = null;
                    return;
                }
                if (_framePtr == value)
                    return;
                _framePtr?.Dispose();
#if UNMANAGE_MONEY_CHECK
                MemoryCheck.SetIsAloc(nameof(ZFrame));
#endif
                _framePtr = value;
            }
        }

        private int _len;

        private int _position;
        private DispoIntPtr _framePtr;

        // private ZeroMQ.lib.FreeMessageDelegate _freePtr;

        public ZFrame(byte[] buffer)
            : this(CreateNative(buffer.Length), buffer.Length)
        {
            Write(buffer, 0, buffer.Length);
        }

        public ZFrame(byte[] buffer, int offset, int count)
            : this(CreateNative(count), count)
        {
            Write(buffer, offset, count);
        }

        public ZFrame(byte value)
            : this(CreateNative(1), 1)
        {
            Write(value);
        }

        public ZFrame(short value)
            : this(CreateNative(2), 2)
        {
            Write(value);
        }

        public ZFrame(ushort value)
            : this(CreateNative(2), 2)
        {
            Write(value);
        }

        public ZFrame(char value)
            : this(CreateNative(2), 2)
        {
            Write(value);
        }

        public ZFrame(int value)
            : this(CreateNative(4), 4)
        {
            Write(value);
        }

        public ZFrame(uint value)
            : this(CreateNative(4), 4)
        {
            Write(value);
        }

        public ZFrame(long value)
            : this(CreateNative(8), 8)
        {
            Write(value);
        }

        public ZFrame(ulong value)
            : this(CreateNative(8), 8)
        {
            Write(value);
        }

        public ZFrame(string str)
            : this(str, ZContext.Encoding)
        { }

        public ZFrame(string str, Encoding encoding)
        {
            WriteStringNative(str, encoding, true);
        }

        public ZFrame()
            : this(CreateNative(0), 0)
        {
        }

        /* protected ZFrame(IntPtr data, int size)
			: this(Alloc(data, size), size)
		{ } */

        internal ZFrame(DispoIntPtr frameIntPtr, int size)
        {
            FramePtr = frameIntPtr;
            _len = size;
            _position = 0;
        }

        ~ZFrame()
        {
            Dispose(false);
        }

        internal static DispoIntPtr CreateEmptyNative()
        {
            var msg = DispoIntPtr.Alloc(zmq.sizeof_zmq_msg_t);

            while (-1 == zmq.msg_init(msg))
            {
                var error = ZError.GetLastErr();

                if (Equals(error, ZError.EINTR))
                {
                    continue;
                }
                msg.Dispose();
                throw new ZException(error, "zmq_msg_init");
            }
            return msg;
        }

        internal static DispoIntPtr CreateNative(int size)
        {
            var msg = DispoIntPtr.Alloc(zmq.sizeof_zmq_msg_t);


            while (-1 == zmq.msg_init_size(msg, size))
            {
                var error = ZError.GetLastErr();

                if (Equals(error, ZError.EINTR))
                {
                    continue;
                }

                msg.Dispose();

                if (Equals(error, ZError.ENOMEM))
                {
                    throw new OutOfMemoryException("zmq_msg_init_size");
                }
                throw new ZException(error, "zmq_msg_init_size");
            }
            return msg;
        }

        /* internal static DispoIntPtr Alloc(IntPtr data, int size) 
		{
			var msg = DispoIntPtr.Alloc(zmq.sizeof_zmq_msg_t);

			ZError error;
			while (-1 == zmq.msg_init_data(msg, data, size, /* msg_free_delegate null, /* hint IntPtr.Zero)) {
				error = ZError.GetLastError();

				if (error == ZError.EINTR) {
					continue;
				}

				msg.Dispose();

				if (error == ZError.ENOMEM) {
					throw new OutOfMemoryException ("zmq_msg_init_size");
				}
				throw new ZException (error, "zmq_msg_init_size");
			}
			return msg;
		} */

        protected override void Dispose(bool disposing)
        {
            Close();
            base.Dispose(disposing);
        }

        public bool IsDismissed => FramePtr == null;

        public override bool CanRead => true;

        public override bool CanSeek => true;

        public override bool CanTimeout => false;

        public override bool CanWrite => true;

        private void EnsureCapacity()
        {
            if (FramePtr != IntPtr.Zero)
            {
                _len = zmq.msg_size(FramePtr);
            }
            else
            {
                _len = -1;
            }
        }

        public override long Length
        {
            get
            {
                EnsureCapacity();
                return _len;
            }
        }

        public override void SetLength(long length)
        {
            throw new NotSupportedException();
        }

        public override long Position
        {
            get => _position;
            set => Seek(value, SeekOrigin.Begin);
        }

        public IntPtr Ptr => FramePtr;

        public IntPtr DataPtr()
        {
            if (FramePtr == IntPtr.Zero)
            {
                return IntPtr.Zero;
            }
            return zmq.msg_data(FramePtr);
        }

        public override long Seek(long offset, SeekOrigin origin)
        {
            long pos;
            if (origin == SeekOrigin.Current)
                pos = _position + offset;
            else if (origin == SeekOrigin.End)
                pos = Length + offset;
            else // if (origin == SeekOrigin.Begin)
                pos = offset;

            if (pos < 0 || pos > Length)
                throw new ArgumentOutOfRangeException(nameof(offset));

            _position = (int)pos;
            return pos;
        }

        public byte[] Read()
        {
            int remaining = Math.Max(0, (int)(Length - _position));
            return Read(remaining);
        }

        public byte[] Read(int count)
        {
            int remaining = Math.Min(count, Math.Max(0, (int)(Length - _position)));
            if (remaining == 0)
            {
                return new byte[0];
            }
            if (remaining < 0)
            {
                return null;
            }
            var bytes = new byte[remaining];
            /* int bytesLength = */
            Read(bytes, 0, remaining);
            return bytes;
        }

        public override int Read(byte[] buffer, int offset, int count)
        {
            int remaining = Math.Min(count, Math.Max(0, (int)(Length - _position)));
            if (remaining == 0)
            {
                return 0;
            }
            if (remaining < 0)
            {
                return -1;
            }
            Marshal.Copy(DataPtr() + _position, buffer, offset, remaining);

            _position += remaining;
            return remaining;
        }

        public override int ReadByte()
        {
            if (_position + 1 > Length)
                return -1;

            int byt = Marshal.ReadByte(DataPtr() + _position);
            ++_position;
            return byt;
        }

        public byte ReadAsByte()
        {
            if (_position + 1 > Length)
                return default(byte);

            byte byt = Marshal.ReadByte(DataPtr() + _position);
            ++_position;
            return byt;
        }

        public short ReadInt16()
        {
            var bytes = new byte[2];
            if (Read(bytes, 0, 2) < 2)
            {
                return default(short);
            }

            return BitConverter.ToInt16(bytes, 0);
        }

        public ushort ReadUInt16()
        {
            var bytes = new byte[2];
            if (Read(bytes, 0, 2) < 2)
            {
                return default(ushort);
            }

            return BitConverter.ToUInt16(bytes, 0);
        }

        public char ReadChar()
        {
            var bytes = new byte[2];
            if (Read(bytes, 0, 2) < 2)
            {
                return default(char);
            }

            return BitConverter.ToChar(bytes, 0);
        }

        public int ReadInt32()
        {
            var bytes = new byte[4];
            int len = Read(bytes, 0, 4);
            if (len < 4)
            {
                return default(int);
            }

            return BitConverter.ToInt32(bytes, 0);
        }

        public uint ReadUInt32()
        {
            var bytes = new byte[4];
            if (Read(bytes, 0, 4) < 4)
            {
                return default(uint);
            }

            return BitConverter.ToUInt32(bytes, 0);
        }

        public long ReadInt64()
        {
            var bytes = new byte[8];
            if (Read(bytes, 0, 8) < 8)
            {
                return default(long);
            }

            return BitConverter.ToInt64(bytes, 0);
        }

        public ulong ReadUInt64()
        {
            var bytes = new byte[8];
            if (Read(bytes, 0, 8) < 8)
            {
                return default(ulong);
            }

            return BitConverter.ToUInt64(bytes, 0);
        }

        public string ReadString()
        {
            return ReadString(ZContext.Encoding);
        }

        public string ReadString(Encoding encoding)
        {
            return ReadString( /* byteCount */ (int)Length - _position, encoding);
        }

        public string ReadString(int length)
        {
            return ReadString(/* byteCount */ length, ZContext.Encoding);
        }

        public string ReadString(int byteCount, Encoding encoding)
        {
            int remaining = Math.Min(byteCount, Math.Max(0, (int)Length - _position));
            if (remaining == 0)
            {
                return string.Empty;
            }
            if (remaining < 0)
            {
                return null;
            }

            unsafe
            {
                var bytes = (byte*)(DataPtr() + _position);

                Decoder dec = encoding.GetDecoder();
                int charCount = dec.GetCharCount(bytes, remaining, false);
                if (charCount == 0)
                {
                    return string.Empty;
                }

                var resultChars = new char[charCount];
                fixed (char* chars = resultChars)
                {
                    charCount = dec.GetChars(bytes, remaining, chars, charCount, true);

                    int i = -1, z = 0;
                    while (i < charCount)
                    {
                        ++i;

                        if (chars[i] == '\0')
                        {
                            charCount = i;
                            ++z;

                            break;
                        }
                    }

                    Encoder enc = encoding.GetEncoder();
                    _position += enc.GetByteCount(chars, charCount + z, true);

                    if (charCount == 0) return string.Empty;
                    return new string(chars, 0, charCount);
                }
            }
        }

        public string ReadLine()
        {
            return ReadLine((int)Length - _position, ZContext.Encoding);
        }

        public string ReadLine(Encoding encoding)
        {
            return ReadLine((int)Length - _position, encoding);
        }

        public string ReadLine(int byteCount, Encoding encoding)
        {
            int remaining = Math.Min(byteCount, Math.Max(0, (int)Length - _position));
            if (remaining == 0)
            {
                return string.Empty;
            }
            if (remaining < 0)
            {
                return null;
            }

            unsafe
            {
                var bytes = (byte*)(DataPtr() + _position);

                Decoder dec = encoding.GetDecoder();
                int charCount = dec.GetCharCount(bytes, remaining, false);
                if (charCount == 0) return string.Empty;

                var resultChars = new char[charCount];
                fixed (char* chars = resultChars)
                {
                    charCount = dec.GetChars(bytes, remaining, chars, charCount, true);

                    int i = -1, z = 0;
                    while (i < charCount)
                    {
                        ++i;

                        if (chars[i] == '\n')
                        {
                            charCount = i;
                            ++z;

                            if (i - 1 > -1 && chars[i - 1] == '\r')
                            {
                                --charCount;
                                ++z;
                            }

                            break;
                        }
                        if (chars[i] == '\0')
                        {
                            charCount = i;
                            ++z;

                            break;
                        }
                    }

                    Encoder enc = encoding.GetEncoder();
                    _position += enc.GetByteCount(chars, charCount + z, true);

                    if (charCount == 0) return string.Empty;
                    return new string(chars, 0, charCount);
                }
            }
        }

        public override void Write(byte[] buffer, int offset, int count)
        {
            if (_position + count > Length)
            {
                throw new InvalidOperationException();
            }
            Marshal.Copy(buffer, offset, DataPtr() + _position, count);
            _position += count;
        }

        public void Write(byte value)
        {
            if (_position + 1 > Length)
            {
                throw new InvalidOperationException();
            }
            Marshal.WriteByte(DataPtr() + _position, value);
            ++_position;
        }

        public override void WriteByte(byte value)
        {
            Write(value);
        }

        public void Write(short value)
        {
            if (_position + 2 > Length)
            {
                throw new InvalidOperationException();
            }
            Marshal.WriteInt16(DataPtr() + _position, value);
            _position += 2;
        }

        public void Write(ushort value)
        {
            Write((short)value);
        }

        public void Write(char value)
        {
            Write((short)value);
        }

        public void Write(int value)
        {
            if (_position + 4 > Length)
            {
                throw new InvalidOperationException();
            }
            Marshal.WriteInt32(DataPtr() + _position, value);
            _position += 4;
        }

        public void Write(uint value)
        {
            Write((int)value);
        }

        public void Write(long value)
        {
            if (_position + 8 > Length)
            {
                throw new InvalidOperationException();
            }
            Marshal.WriteInt64(DataPtr() + _position, value);
            _position += 8;
        }

        public void Write(ulong value)
        {
            Write((long)value);
        }

        public void Write(string str)
        {
            Write(str, ZContext.Encoding);
        }

        public void Write(string str, Encoding encoding)
        {
            if (FramePtr != null)
            {
                Close();
            }
            WriteStringNative(str, encoding, false);
        }

        public void WriteLine(string str)
        {
            WriteLine(str, ZContext.Encoding);
        }

        public void WriteLine(string str, Encoding encoding)
        {
            if (FramePtr != null)
            {
                Close();
            }
            WriteStringNative($"{str}\r\n", encoding, false);
        }

        internal unsafe void WriteStringNative(string str, Encoding encoding, bool create)
        {
            if (string.IsNullOrEmpty(str))
            {
                if (create)
                {
                    FramePtr = CreateNative(0);
                    _len = 0;
                    _position = 0;
                }
                return;
            }

            int charCount = str.Length;
            Encoder enc = encoding.GetEncoder();

            fixed (char* strP = str)
            {
                int byteCount = enc.GetByteCount(strP, charCount, false);

                if (create)
                {
                    FramePtr = CreateNative(byteCount);
                    _len = byteCount;
                    _position = 0;
                }
                else if (_position + byteCount > Length)
                {
                    // fail if frame is too small
                    throw new InvalidOperationException();
                }

                byteCount = enc.GetBytes(strP, charCount, (byte*)(DataPtr() + _position), byteCount, true);
                _position += byteCount;
            }
        }

        public override void Flush()
        {
            throw new NotSupportedException();
        }

        public override void Close()
        {
            if (_framePtr == null)
                return;
            var ptr = _framePtr;
            _framePtr = null;
            if (ptr.IsDisposed)
                return;
            //ZError error;
            while (-1 == zmq.msg_close(ptr))
            {
                if (Equals(ZError.GetLastErr(), ZError.EINTR))
                {
                    //error = default(ZError);
                    continue;
                }
                break;
            }

            ptr.Dispose();
#if UNMANAGE_MONEY_CHECK
            MemoryCheck.SetIsFree(nameof(ZFrame));
#endif
        }

        public void CopyZeroTo(ZFrame other)
        {
            while (-1 == zmq.msg_copy(other.FramePtr, FramePtr))
            {
                // zmq.msg_copy(dest, src)
                ZError error = ZError.GetLastErr();

                if (Equals(error, ZError.EINTR))
                {
                    error = default(ZError);
                    continue;
                }
                if (error == ZError.EFAULT)
                {
                    // Invalid message. 
                }
                throw new ZException(error, "zmq_msg_copy");
            }
        }

        public void MoveZeroTo(ZFrame other)
        {
            // zmq.msg_copy(dest, src)
            while (-1 == zmq.msg_move(other.FramePtr, FramePtr))
            {
                var error = ZError.GetLastErr();

                if (error == ZError.EINTR)
                {
                    error = default(ZError);
                    continue;
                }
                if (error == ZError.EFAULT)
                {
                    // Invalid message. 
                }
                throw new ZException(error, "zmq_msg_move");
            }

            // When move, msg_close this _framePtr
            Close();
        }

        public int GetOption(ZFrameOption property)
        {
            int result;
            if (-1 == (result = GetOption(property, out ZError error)))
            {
                throw new ZException(error);
            }
            return result;
        }

        public int GetOption(ZFrameOption property, out ZError error)
        {
            error = ZError.None;

            int result;
            if (-1 == (result = zmq.msg_get(FramePtr, (int)property)))
            {
                error = ZError.GetLastErr();
                return -1;
            }
            return result;
        }

        public string GetOption(string property)
        {
            string result;
            if (null == (result = GetOption(property, out ZError error)))
            {
                if (error != ZError.None)
                {
                    throw new ZException(error);
                }
            }
            return result;
        }

        public string GetOption(string property, out ZError error)
        {
            error = ZError.None;

            string result = null;
            using (var propertyPtr = DispoIntPtr.AllocString(property))
            {
                IntPtr resultPtr;
                if (IntPtr.Zero == (resultPtr = zmq.msg_gets(FramePtr, propertyPtr)))
                {
                    error = ZError.GetLastErr();
                    return null;
                }
                else
                {
                    Marshal.FreeHGlobal(resultPtr);
                    result = Marshal.PtrToStringAnsi(resultPtr);
                }
            }
            return result;
        }

        #region ICloneable implementation

        public object Clone()
        {
            return Duplicate();
        }

        /// <summary>
        /// 重用
        /// </summary>
        public ZFrame Duplicate()
        {
            var frame = CreateEmpty();
            CopyZeroTo(frame);
            return frame;
        }

        #endregion

        public override string ToString()
        {
            return ToString(ZContext.Encoding);
        }

        public string ToString(Encoding encoding)
        {
            if (Length > -1)
            {
                long old = _position;
                Position = 0;
                string retur = ReadString(encoding);
                Position = old;
                return retur;
            }
            return GetType().FullName;
        }
    }
}
#pragma warning restore CS1591 // 缺少对公共可见类型或成员的 XML 注释