using Agebull.Common.Logging;
using System;
using System.IO;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Agebull.MicroZero.ZeroApis
{
    /// <summary>
    /// 替换Response的Stream可读写流对象
    /// </summary>
    public class MemoryWrappedHttpResponseStream : MemoryStream
    {
        private Stream _innerStream;
        /// <summary>
        /// 构造
        /// </summary>
        /// <param name="innerStream"></param>
        public MemoryWrappedHttpResponseStream(Stream innerStream)
        {
            this._innerStream = innerStream ?? throw new ArgumentNullException(nameof(innerStream));
        }
        /// <inheritdoc/>
        public override void Flush()
        {
            this._innerStream.Flush();
            base.Flush();
        }

        /// <inheritdoc/>
        public override void Write(byte[] buffer, int offset, int count)
        {
            var json = Encoding.UTF8.GetString(buffer, offset, count);
            LogRecorder.MonitorTrace(json);
            //base.Write(buffer, offset, count);
            //this._innerStream.WriteAsync(buffer, offset, count).Wait();
        }

        ///// <inheritdoc/>
        //public override void Write(ReadOnlySpan<byte> source)
        //{
        //    base.Write(source);
        //   // this._innerStream.Write(source);
        //}

        /// <inheritdoc/>
        public override async Task WriteAsync(byte[] buffer, int offset, int count, CancellationToken cancellationToken)
        {
            await base.WriteAsync(buffer, offset, count, cancellationToken);
            await _innerStream.WriteAsync(buffer, offset, count, cancellationToken);
        }
        ///// <inheritdoc/>
        //public override async ValueTask WriteAsync(ReadOnlyMemory<byte> source, CancellationToken cancellationToken = default)
        //{
        //    await base.WriteAsync(source, cancellationToken);
        //    await this._innerStream.WriteAsync(source);
        //}

        /// <inheritdoc/>
        protected override void Dispose(bool disposing)
        {
            base.Dispose(disposing);
            if (disposing)
            {
                this._innerStream.Dispose();
            }
        }

        /// <inheritdoc/>
        public override void Close()
        {
            base.Close();
            this._innerStream.Close();
        }
    }
}