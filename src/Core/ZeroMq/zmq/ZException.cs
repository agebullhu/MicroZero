namespace ZeroMQ
{
    using System;
    using System.Runtime.Serialization;

    /// <summary>
    /// An exception thrown by the result of libzmq.
    /// </summary>
    [Serializable]
	public class ZException : Exception
	{
	    /// <summary>
        /// Gets the error code returned by libzmq.
        /// </summary>
        [Obsolete("Use Error property instead")]
        public int ErrNo => Error?.Number ?? 0;

	    /// <summary>
        /// Gets the error code returned by libzmq.
        /// </summary>
        [Obsolete("Use Error property instead")]
        public string ErrName => Error != null ? Error.Name : string.Empty;

	    /// <summary>
        /// Gets the error text returned by libzmq.
        /// </summary>
        [Obsolete("Use Error property instead")]
        public string ErrText => Error != null ? Error.Text : string.Empty;
        /// <summary>
        /// Error Object
        /// </summary>
	    public ZError Error { get; }

	    /// <summary>
        /// Initializes a new instance of the <see cref="ZException"/> class.
        /// </summary>
        protected ZException()
		{ }

        /// <summary>
        /// Initializes a new instance of the <see cref="ZException"/> class.
        /// </summary>
        /// <param name="errorSymbol">The error code returned by the ZeroMQ library call.</param>
        public ZException(ZError errorSymbol)
			: this(errorSymbol, default, default)
		{ }

	    /// <summary>
	    /// Initializes a new instance of the <see cref="ZException"/> class.
	    /// </summary>
	    /// <param name="errorSymbol">The error code returned by the ZeroMQ library call.</param>
	    /// <param name="message"></param>
	    public ZException(ZError errorSymbol, string message)
			: this(errorSymbol, message, default)
		{ }

	    /// <inheritdoc />
	    public ZException(ZError errorSymbol, string message, Exception inner)
			: base(MakeMessage(errorSymbol, message), inner)
		{
		    Error = errorSymbol;
		}

		static string MakeMessage(ZError error, string additionalMessage)
		{
		    return error != null
		        ? (string.IsNullOrEmpty(additionalMessage)
		            ? error.ToString()
		            : $"{error}: {additionalMessage}")
		        : additionalMessage;
		}

#pragma warning disable CS1591 // 缺少对公共可见类型或成员的 XML 注释
	    public override string ToString()
#pragma warning restore CS1591 // 缺少对公共可见类型或成员的 XML 注释
		{
			return Message;
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="ZException"/> class.
		/// </summary>
		/// <param name="info"><see cref="SerializationInfo"/> that holds the serialized object data about the exception being thrown.</param>
		/// <param name="context"><see cref="StreamingContext"/> that contains contextual information about the source or destination.</param>
		protected ZException(SerializationInfo info, StreamingContext context)
			: base(info, context)
		{ }

	}
}