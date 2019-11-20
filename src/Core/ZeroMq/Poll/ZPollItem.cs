namespace ZeroMQ
{
    /// <summary>
    /// pool节点
    /// </summary>
    public class ZPollItem
	{
#pragma warning disable CS1591 // 缺少对公共可见类型或成员的 XML 注释
		public ZPollEvent Events;
#pragma warning restore CS1591 // 缺少对公共可见类型或成员的 XML 注释

#pragma warning disable CS1591 // 缺少对公共可见类型或成员的 XML 注释
		public ZPollEvent ReadyEvents;
#pragma warning restore CS1591 // 缺少对公共可见类型或成员的 XML 注释

#pragma warning disable CS1591 // 缺少对公共可见类型或成员的 XML 注释
		public delegate bool ReceiveDelegate(ZSocket socket, out ZMessage message, out ZError error);
#pragma warning restore CS1591 // 缺少对公共可见类型或成员的 XML 注释

#pragma warning disable CS1591 // 缺少对公共可见类型或成员的 XML 注释
		public ReceiveDelegate ReceiveMessage;
#pragma warning restore CS1591 // 缺少对公共可见类型或成员的 XML 注释

#pragma warning disable CS1591 // 缺少对公共可见类型或成员的 XML 注释
		public static bool DefaultReceiveMessage(ZSocket socket, out ZMessage message, out ZError error)
#pragma warning restore CS1591 // 缺少对公共可见类型或成员的 XML 注释
		{
			message = null;
			return socket.ReceiveMessage(ref message, out error);
		}

#pragma warning disable CS1591 // 缺少对公共可见类型或成员的 XML 注释
		public delegate bool SendDelegate(ZSocket socket, ZMessage message, out ZError error);
#pragma warning restore CS1591 // 缺少对公共可见类型或成员的 XML 注释

#pragma warning disable CS1591 // 缺少对公共可见类型或成员的 XML 注释
		public SendDelegate SendMessage;
#pragma warning restore CS1591 // 缺少对公共可见类型或成员的 XML 注释

#pragma warning disable CS1591 // 缺少对公共可见类型或成员的 XML 注释
		public static bool DefaultSendMessage(ZSocket socket, ZMessage message, out ZError error)
#pragma warning restore CS1591 // 缺少对公共可见类型或成员的 XML 注释
		{
			return socket.Send(message, out error);
		}

#pragma warning disable CS1591 // 缺少对公共可见类型或成员的 XML 注释
	    public ZPollItem(ZPollEvent events)
#pragma warning restore CS1591 // 缺少对公共可见类型或成员的 XML 注释
		{
			Events = events;
		}

#pragma warning disable CS1591 // 缺少对公共可见类型或成员的 XML 注释
		public static ZPollItem Create(ReceiveDelegate receiveMessage)
#pragma warning restore CS1591 // 缺少对公共可见类型或成员的 XML 注释
		{
			return Create(receiveMessage, null);
		}

#pragma warning disable CS1591 // 缺少对公共可见类型或成员的 XML 注释
		public static ZPollItem CreateSender(SendDelegate sendMessage)
#pragma warning restore CS1591 // 缺少对公共可见类型或成员的 XML 注释
		{
			return Create(null, sendMessage);
		}

#pragma warning disable CS1591 // 缺少对公共可见类型或成员的 XML 注释
		public static ZPollItem Create(ReceiveDelegate receiveMessage, SendDelegate sendMessage)
#pragma warning restore CS1591 // 缺少对公共可见类型或成员的 XML 注释
		{
			var pollItem = new ZPollItem((receiveMessage != null ? ZPollEvent.In : ZPollEvent.None) | (sendMessage != null ? ZPollEvent.Out : ZPollEvent.None));
			pollItem.ReceiveMessage = receiveMessage;
			pollItem.SendMessage = sendMessage;
			return pollItem;
		}

#pragma warning disable CS1591 // 缺少对公共可见类型或成员的 XML 注释
		public static ZPollItem CreateReceiver()
#pragma warning restore CS1591 // 缺少对公共可见类型或成员的 XML 注释
		{
			return Create(DefaultReceiveMessage, null);
		}

#pragma warning disable CS1591 // 缺少对公共可见类型或成员的 XML 注释
		public static ZPollItem CreateSender()
#pragma warning restore CS1591 // 缺少对公共可见类型或成员的 XML 注释
		{
			return Create(null, DefaultSendMessage);
		}

#pragma warning disable CS1591 // 缺少对公共可见类型或成员的 XML 注释
		public static ZPollItem CreateReceiverSender()
#pragma warning restore CS1591 // 缺少对公共可见类型或成员的 XML 注释
		{
			return Create(DefaultReceiveMessage, DefaultSendMessage);
		}
	}
}