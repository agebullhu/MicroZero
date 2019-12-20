namespace ZeroMQ
{
#pragma warning disable CS1591 // 缺少对公共可见类型或成员的 XML 注释
    public delegate void ZAction0(ZSocket backend, System.Threading.CancellationTokenSource cancellor, object[] args);
#pragma warning restore CS1591 // 缺少对公共可见类型或成员的 XML 注释

#pragma warning disable CS1591 // 缺少对公共可见类型或成员的 XML 注释
	public delegate void ZAction(ZContext context, ZSocket backend, System.Threading.CancellationTokenSource cancellor, object[] args);
#pragma warning restore CS1591 // 缺少对公共可见类型或成员的 XML 注释

#pragma warning disable CS1591 // 缺少对公共可见类型或成员的 XML 注释
	public class ZActor : ZThread
#pragma warning restore CS1591 // 缺少对公共可见类型或成员的 XML 注释
	{
#pragma warning disable CS1591 // 缺少对公共可见类型或成员的 XML 注释
		public ZContext Context { get; protected set; }
#pragma warning restore CS1591 // 缺少对公共可见类型或成员的 XML 注释

#pragma warning disable CS1591 // 缺少对公共可见类型或成员的 XML 注释
		public string Endpoint { get; protected set; }
#pragma warning restore CS1591 // 缺少对公共可见类型或成员的 XML 注释

#pragma warning disable CS1591 // 缺少对公共可见类型或成员的 XML 注释
		public ZAction Action { get; protected set; }
#pragma warning restore CS1591 // 缺少对公共可见类型或成员的 XML 注释

#pragma warning disable CS1591 // 缺少对公共可见类型或成员的 XML 注释
		public ZAction0 Action0 { get; protected set; }
#pragma warning restore CS1591 // 缺少对公共可见类型或成员的 XML 注释

#pragma warning disable CS1591 // 缺少对公共可见类型或成员的 XML 注释
		public object[] Arguments { get; protected set; }
#pragma warning restore CS1591 // 缺少对公共可见类型或成员的 XML 注释

#pragma warning disable CS1591 // 缺少对公共可见类型或成员的 XML 注释
		public ZSocket Backend { get; protected set; }
#pragma warning restore CS1591 // 缺少对公共可见类型或成员的 XML 注释

#pragma warning disable CS1591 // 缺少对公共可见类型或成员的 XML 注释
		public ZSocket Frontend { get; protected set; }
#pragma warning restore CS1591 // 缺少对公共可见类型或成员的 XML 注释

#pragma warning disable CS1591 // 缺少对公共可见类型或成员的 XML 注释
		public ZActor(ZContext context, ZAction action, params object[] args)
#pragma warning restore CS1591 // 缺少对公共可见类型或成员的 XML 注释
			: this(context, default, action, args)
		{
			var rnd0 = new byte[8];
			using (var rng = new System.Security.Cryptography.RNGCryptoServiceProvider()) rng.GetNonZeroBytes(rnd0);
			Endpoint = $"inproc://{ZContext.Encoding.GetString(rnd0)}";
		}

#pragma warning disable CS1591 // 缺少对公共可见类型或成员的 XML 注释
		public ZActor(ZContext context, string endpoint, ZAction action, params object[] args)
#pragma warning restore CS1591 // 缺少对公共可见类型或成员的 XML 注释
		{
			Context = context;

			Endpoint = endpoint;
			Action = action;
			Arguments = args;
		}

		/// <summary>
		/// You are using ZContext.Current!
		/// </summary>
		public ZActor(ZAction0 action, params object[] args)
			: this(default, action, args)
		{
			var rnd0 = new byte[8];
			using (var rng = new System.Security.Cryptography.RNGCryptoServiceProvider()) rng.GetNonZeroBytes(rnd0);
			Endpoint = $"inproc://{ZContext.Encoding.GetString(rnd0)}";
		}

		/// <summary>
		/// You are using ZContext.Current!
		/// </summary>
		public ZActor(string endpoint, ZAction0 action, params object[] args)
		{
			Context = ZContext.Current;

			Endpoint = endpoint;
			Action0 = action;
			Arguments = args;
		}

#pragma warning disable CS1591 // 缺少对公共可见类型或成员的 XML 注释
		protected override void Run()
#pragma warning restore CS1591 // 缺少对公共可见类型或成员的 XML 注释
		{
			using (Backend = ZSocket.Create(Context, ZSocketType.PAIR))
			{
				Backend.Bind(Endpoint);

				if (Action0 != null)
				{
					Action0(Backend, Cancellor, Arguments);
				}
				if (Action != null)
				{
					Action(Context, Backend, Cancellor, Arguments);
				}
			}
		}

#pragma warning disable CS1591 // 缺少对公共可见类型或成员的 XML 注释
		public override void Start()
#pragma warning restore CS1591 // 缺少对公共可见类型或成员的 XML 注释
		{
			base.Start();

			if (Frontend == null)
			{
				Frontend = ZSocket.Create(Context, ZSocketType.PAIR);
				Frontend.Connect(Endpoint);
			}
		}

#pragma warning disable CS1591 // 缺少对公共可见类型或成员的 XML 注释
		protected override void Dispose(bool disposing)
#pragma warning restore CS1591 // 缺少对公共可见类型或成员的 XML 注释
		{
			base.Dispose(disposing);

			if (disposing)
			{
				if (Frontend != null)
				{
					Frontend.Dispose();
					Frontend = null;
				}
				if (Backend != null)
				{
					Backend.Dispose();
					Backend = null;
				}
			}
		}
	}
}