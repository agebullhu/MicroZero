using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;

namespace ZeroMQ
{
    /// <summary>
    /// A single or multi-part message, sent or received via a <see cref="ZSocket"/>.
    /// </summary>
    public class ZMessage : MemoryCheck, IList<ZFrame>, ICloneable
    {
#if UNMANAGE_MONEY_CHECK
        protected override string TypeName => nameof(ZMessage);
#endif
        private List<ZFrame> _frames;

        /// <summary>
        /// Initializes a new instance of the <see cref="ZMessage"/> class.
        /// Creates an empty message.
        /// </summary>
        public ZMessage()
        {
            _frames = new List<ZFrame>();
        }
        /// <summary>
        /// Initializes a new instance of the <see cref="ZMessage"/> class.
        /// Creates an empty message.
        /// </summary>
        public ZMessage(byte[] desc, params string[] args)
        {
            _frames = new List<ZFrame> { new ZFrame(desc) };
            if (args != null)
                foreach (var frame in args)
                    _frames.Add(new ZFrame(frame));
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ZMessage"/> class.
        /// Creates an empty message.
        /// </summary>
        public ZMessage(byte[] desc, params byte[][] args)
        {
            _frames = new List<ZFrame> { new ZFrame(desc) };
            if (args != null)
                foreach (var frame in args)
                    _frames.Add(new ZFrame(frame));
        }
        /// <summary>
        /// Initializes a new instance of the <see cref="ZMessage"/> class.
        /// Creates an empty message.
        /// </summary>
        public static ZMessage Create(params byte[][] frames)
        {
            return new ZMessage(frames);
        }
        /// <summary>
        /// Initializes a new instance of the <see cref="ZMessage"/> class.
        /// Creates a message that contains the given <see cref="ZFrame"/> objects.
        /// </summary>
        /// <param name="frames">A collection of <see cref="ZFrame"/> objects to be stored by this <see cref="ZMessage"/>.</param>
        /// <exception cref="ArgumentNullException"><paramref name="frames"/> is null.</exception>
        public ZMessage(IEnumerable<byte[]> frames)
        {
            _frames = new List<ZFrame>();
            foreach (var frame in frames)
                _frames.Add(new ZFrame(frame));
        }
        /// <summary>
        /// Initializes a new instance of the <see cref="ZMessage"/> class.
        /// Creates a message that contains the given <see cref="ZFrame"/> objects.
        /// </summary>
        /// <param name="frames">A collection of <see cref="ZFrame"/> objects to be stored by this <see cref="ZMessage"/>.</param>
        /// <exception cref="ArgumentNullException"><paramref name="frames"/> is null.</exception>
        public ZMessage(IEnumerable<ZFrame> frames)
        {
            if (frames == null)
            {
                throw new ArgumentNullException(nameof(frames));
            }

            _frames = new List<ZFrame>(frames);
        }

        /// <summary>
        /// 析构
        /// </summary>
        protected override void DoDispose()
        {
            if (_frames != null)
            {
                foreach (var frame in _frames)
                {
                    frame.Close();
                }
            }
            _frames = null;
        }

#pragma warning disable CS1591 // 缺少对公共可见类型或成员的 XML 注释
        public void ReplaceAt(int index, ZFrame replacement)
#pragma warning restore CS1591 // 缺少对公共可见类型或成员的 XML 注释
        {
            ReplaceAt(index, replacement, true);
        }

#pragma warning disable CS1591 // 缺少对公共可见类型或成员的 XML 注释
        public ZFrame ReplaceAt(int index, ZFrame replacement, bool dispose)
#pragma warning restore CS1591 // 缺少对公共可见类型或成员的 XML 注释
        {
            var old = _frames[index];
            _frames[index] = replacement;
            if (dispose)
            {
                old.Dispose();
                return null;
            }
            return old;
        }

        #region IList implementation

#pragma warning disable CS1591 // 缺少对公共可见类型或成员的 XML 注释
        public int IndexOf(ZFrame item)
#pragma warning restore CS1591 // 缺少对公共可见类型或成员的 XML 注释
        {
            return _frames.IndexOf(item);
        }

#pragma warning disable CS1591 // 缺少对公共可见类型或成员的 XML 注释
        public void Prepend(ZFrame item)
#pragma warning restore CS1591 // 缺少对公共可见类型或成员的 XML 注释
        {
            Insert(0, item);
        }

#pragma warning disable CS1591 // 缺少对公共可见类型或成员的 XML 注释
        public void Insert(int index, ZFrame item)
#pragma warning restore CS1591 // 缺少对公共可见类型或成员的 XML 注释
        {
            _frames.Insert(index, item);
        }

        /// <summary>
        /// Removes ZFrames. Note: Disposes the ZFrame.
        /// </summary>
        /// <returns>The <see cref="ZeroMQ.ZFrame"/>.</returns>
        public void RemoveAt(int index)
        {
            RemoveAt(index, true);
        }

        /// <summary>
        /// Removes ZFrames.
        /// </summary>
        /// <returns>The <see cref="ZeroMQ.ZFrame"/>.</returns>
        /// <param name="index"></param>
        /// <param name="dispose">If set to <c>false</c>, do not dispose the ZFrame.</param>
        public ZFrame RemoveAt(int index, bool dispose)
        {
            var frame = _frames[index];
            _frames.RemoveAt(index);

            if (!dispose)
                return frame;
            frame.Dispose();
            return null;
        }

#pragma warning disable CS1591 // 缺少对公共可见类型或成员的 XML 注释
        public ZFrame Pop()
#pragma warning restore CS1591 // 缺少对公共可见类型或成员的 XML 注释
        {
            var result = RemoveAt(0, false);
            result.Position = 0; // TODO maybe remove this here again, see https://github.com/zeromq/clrzmq4/issues/110
            return result;
        }

#pragma warning disable CS1591 // 缺少对公共可见类型或成员的 XML 注释
        public int PopBytes(byte[] buffer, int offset, int count)
#pragma warning restore CS1591 // 缺少对公共可见类型或成员的 XML 注释
        {
            using (var frame = Pop())
            {
                return frame.Read(buffer, offset, count);
            }
        }

#pragma warning disable CS1591 // 缺少对公共可见类型或成员的 XML 注释
        public int PopByte()
#pragma warning restore CS1591 // 缺少对公共可见类型或成员的 XML 注释
        {
            using (var frame = Pop())
            {
                return frame.ReadByte();
            }
        }

#pragma warning disable CS1591 // 缺少对公共可见类型或成员的 XML 注释
        public byte PopAsByte()
#pragma warning restore CS1591 // 缺少对公共可见类型或成员的 XML 注释
        {
            using (var frame = Pop())
            {
                return frame.ReadAsByte();
            }
        }

#pragma warning disable CS1591 // 缺少对公共可见类型或成员的 XML 注释
        public Int16 PopInt16()
#pragma warning restore CS1591 // 缺少对公共可见类型或成员的 XML 注释
        {
            using (var frame = Pop())
            {
                return frame.ReadInt16();
            }
        }

#pragma warning disable CS1591 // 缺少对公共可见类型或成员的 XML 注释
        public UInt16 PopUInt16()
#pragma warning restore CS1591 // 缺少对公共可见类型或成员的 XML 注释
        {
            using (var frame = Pop())
            {
                return frame.ReadUInt16();
            }
        }

#pragma warning disable CS1591 // 缺少对公共可见类型或成员的 XML 注释
        public Char PopChar()
#pragma warning restore CS1591 // 缺少对公共可见类型或成员的 XML 注释
        {
            using (var frame = Pop())
            {
                return frame.ReadChar();
            }
        }

#pragma warning disable CS1591 // 缺少对公共可见类型或成员的 XML 注释
        public Int32 PopInt32()
#pragma warning restore CS1591 // 缺少对公共可见类型或成员的 XML 注释
        {
            using (var frame = Pop())
            {
                return frame.ReadInt32();
            }
        }

#pragma warning disable CS1591 // 缺少对公共可见类型或成员的 XML 注释
        public UInt32 PopUInt32()
#pragma warning restore CS1591 // 缺少对公共可见类型或成员的 XML 注释
        {
            using (var frame = Pop())
            {
                return frame.ReadUInt32();
            }
        }

#pragma warning disable CS1591 // 缺少对公共可见类型或成员的 XML 注释
        public Int64 PopInt64()
#pragma warning restore CS1591 // 缺少对公共可见类型或成员的 XML 注释
        {
            using (var frame = Pop())
            {
                return frame.ReadInt64();
            }
        }

#pragma warning disable CS1591 // 缺少对公共可见类型或成员的 XML 注释
        public UInt64 PopUInt64()
#pragma warning restore CS1591 // 缺少对公共可见类型或成员的 XML 注释
        {
            using (var frame = Pop())
            {
                return frame.ReadUInt64();
            }
        }

#pragma warning disable CS1591 // 缺少对公共可见类型或成员的 XML 注释
        public String PopString()
#pragma warning restore CS1591 // 缺少对公共可见类型或成员的 XML 注释
        {
            return PopString(ZContext.Encoding);
        }

#pragma warning disable CS1591 // 缺少对公共可见类型或成员的 XML 注释
        public String PopString(Encoding encoding)
#pragma warning restore CS1591 // 缺少对公共可见类型或成员的 XML 注释
        {
            using (var frame = Pop())
            {
                return frame.ReadString((int)frame.Length, encoding);
            }
        }

#pragma warning disable CS1591 // 缺少对公共可见类型或成员的 XML 注释
        public String PopString(int bytesCount, Encoding encoding)
#pragma warning restore CS1591 // 缺少对公共可见类型或成员的 XML 注释
        {
            using (var frame = Pop())
            {
                return frame.ReadString(bytesCount, encoding);
            }
        }

#pragma warning disable CS1591 // 缺少对公共可见类型或成员的 XML 注释
        public void Wrap(ZFrame frame)
#pragma warning restore CS1591 // 缺少对公共可见类型或成员的 XML 注释
        {
            Insert(0, new ZFrame());
            Insert(0, frame);
        }

#pragma warning disable CS1591 // 缺少对公共可见类型或成员的 XML 注释
        public ZFrame Unwrap()
#pragma warning restore CS1591 // 缺少对公共可见类型或成员的 XML 注释
        {
            var frame = RemoveAt(0, false);

            if (Count > 0 && this[0].Length == 0)
            {
                RemoveAt(0);
            }

            return frame;
        }

#pragma warning disable CS1591 // 缺少对公共可见类型或成员的 XML 注释
        public ZFrame this[int index]
#pragma warning restore CS1591 // 缺少对公共可见类型或成员的 XML 注释
        {
            get => _frames[index];
            set => _frames[index] = value;
        }

        #endregion

        #region ICollection implementation

#pragma warning disable CS1591 // 缺少对公共可见类型或成员的 XML 注释
        public void Append(ZFrame item)
#pragma warning restore CS1591 // 缺少对公共可见类型或成员的 XML 注释
        {
            Add(item);
        }

#pragma warning disable CS1591 // 缺少对公共可见类型或成员的 XML 注释
        public void AppendRange(IEnumerable<ZFrame> items)
#pragma warning restore CS1591 // 缺少对公共可见类型或成员的 XML 注释
        {
            AddRange(items);
        }

#pragma warning disable CS1591 // 缺少对公共可见类型或成员的 XML 注释
        public void Add(ZFrame item)
#pragma warning restore CS1591 // 缺少对公共可见类型或成员的 XML 注释
        {
            _frames.Add(item);
        }

#pragma warning disable CS1591 // 缺少对公共可见类型或成员的 XML 注释
        public void AddRange(IEnumerable<ZFrame> items)
#pragma warning restore CS1591 // 缺少对公共可见类型或成员的 XML 注释
        {
            _frames.AddRange(items);
        }

#pragma warning disable CS1591 // 缺少对公共可见类型或成员的 XML 注释
        public void Clear()
#pragma warning restore CS1591 // 缺少对公共可见类型或成员的 XML 注释
        {
            foreach (var frame in _frames)
            {
                frame.Dispose();
            }
            _frames.Clear();
        }

#pragma warning disable CS1591 // 缺少对公共可见类型或成员的 XML 注释
        public bool Contains(ZFrame item)
#pragma warning restore CS1591 // 缺少对公共可见类型或成员的 XML 注释
        {
            return _frames.Contains(item);
        }

        void ICollection<ZFrame>.CopyTo(ZFrame[] array, int arrayIndex)
        {
            int i = 0, count = Count;
            foreach (var frame in this)
            {
                array[arrayIndex + i] = ZFrame.CopyFrom(frame);

                i++; if (i >= count) break;
            }
        }

#pragma warning disable CS1591 // 缺少对公共可见类型或成员的 XML 注释
        public bool Remove(ZFrame item)
#pragma warning restore CS1591 // 缺少对公共可见类型或成员的 XML 注释
        {
            if (null != Remove(item, true))
            {
                return false;
            }
            return true;
        }

#pragma warning disable CS1591 // 缺少对公共可见类型或成员的 XML 注释
        public ZFrame Remove(ZFrame item, bool dispose)
#pragma warning restore CS1591 // 缺少对公共可见类型或成员的 XML 注释
        {
            if (_frames.Remove(item))
            {
                if (dispose)
                {
                    item.Dispose();
                    return null;
                }
            }
            return item;
        }
        /// <summary>
        /// 总数
        /// </summary>
        public int Count
        {
            get
            {
                if (_frames == null)
                    return 0;

                return _frames.Count;
            }
        }

        bool ICollection<ZFrame>.IsReadOnly => false;

        #endregion

        #region IEnumerable implementation

#pragma warning disable CS1591 // 缺少对公共可见类型或成员的 XML 注释
        public IEnumerator<ZFrame> GetEnumerator()
#pragma warning restore CS1591 // 缺少对公共可见类型或成员的 XML 注释
        {
            return _frames.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
        #endregion

        #region ICloneable implementation

#pragma warning disable CS1591 // 缺少对公共可见类型或成员的 XML 注释
        public object Clone()
#pragma warning restore CS1591 // 缺少对公共可见类型或成员的 XML 注释
        {
            return Duplicate();
        }

        /// <summary>
        /// 重用
        /// </summary>
        /// <returns></returns>
        public ZMessage Duplicate()
        {
            var message = new ZMessage();
            foreach (var frame in this)
            {
                message.Add(frame.Duplicate());
            }
            return message;
        }

        /// <summary>
        /// 重用
        /// </summary>
        /// <returns></returns>
        public ZMessage Duplicate(int start)
        {
            var message = new ZMessage();
            for (var index = start; index < this.Count; index++)
            {
                var frame = this[index];
                message.Add(frame.Duplicate());
            }

            return message;
        }
#pragma warning disable CS1591 // 缺少对公共可见类型或成员的 XML 注释
        public override string ToString()
#pragma warning restore CS1591 // 缺少对公共可见类型或成员的 XML 注释
        {
            var co = new StringBuilder();
            co.AppendLine($"Frames:{Count}");
            foreach (var f in _frames)
            {
                co.AppendLine(f.ReadString());
            }
            return co.ToString();
        }

        #endregion
    }
}