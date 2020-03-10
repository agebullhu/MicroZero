namespace ZeroMQ
{
#pragma warning disable CS1591 // 缺少对公共可见类型或成员的 XML 注释
    using System;
	using System.Collections.Generic;
	using System.Linq;
	using System.Reflection;
	using System.Runtime.InteropServices;
	using lib;

    /// <summary>
    /// ZError
    /// </summary>
    public class ZSymbol
	{
		protected internal ZSymbol(int errno)
		{
			Number = errno;
		}

	    public int Number { get; }

	    public string Name => ZsymbolToName.TryGetValue(this, out var result) ? result : "<unknown>";

	    public string Text => Marshal.PtrToStringAnsi(zmq.strerror(Number));

	    private static void PickupConstantSymbols<T>(ref IDictionary<ZSymbol, string> symbols)
            where T : ZSymbol
		{
			var type = typeof(T);

			var fields = type.GetFields(BindingFlags.Static | BindingFlags.Public);

			var codeType = type.GetNestedType("Code", BindingFlags.NonPublic);

			// Pickup constant symbols
			foreach (var symbolField in fields.Where(f => typeof(ZSymbol).IsAssignableFrom(f.FieldType)))
			{
				var symbolCodeField = codeType.GetField(symbolField.Name);
				if (symbolCodeField != null)
				{
					var symbolNumber = (int)symbolCodeField.GetValue(null);

				    var symbol = Activator.CreateInstance(
				        type,
				        BindingFlags.NonPublic | BindingFlags.Instance, 
				        null,
				        new object[] {symbolNumber},
				        null);
					symbolField.SetValue(null, symbol);
					symbols.Add((ZSymbol)symbol, symbolCodeField.Name);
				}
			}
		}

		public static readonly ZSymbol None = default(ZSymbol);

		static ZSymbol()
		{
			// System.Runtime.CompilerServices.RuntimeHelpers.RunClassConstructor(typeof(zmq).TypeHandle);

			IDictionary<ZSymbol, string> symbols = new Dictionary<ZSymbol, string>();

			PickupConstantSymbols<ZError>(ref symbols);

			ZsymbolToName = symbols;
		}

		static readonly IDictionary<ZSymbol, string> ZsymbolToName;

		public static IEnumerable<ZSymbol> Find(string symbol)
		{
			return ZsymbolToName
				.Where(s => s.Value != null && (s.Value == symbol)).Select(x => x.Key);
		}

		public static IEnumerable<ZSymbol> Find(string ns, int num)
		{
			return ZsymbolToName
				.Where(s => s.Value != null && (s.Value.StartsWith(ns) && s.Key.Number == num)).Select(x => x.Key);
		}

		public override bool Equals(object obj)
		{
			return Equals(this, obj);
		}

	    public new static bool Equals(object a, object b)
	    {
	        if (ReferenceEquals(a, b))
	        {
	            return true;
	        }

	        return a is ZSymbol symbolA && b is ZSymbol symbolB && symbolA.Number == symbolB.Number;
	    }

		public override int GetHashCode()
		{
			return Number.GetHashCode();
		}

		public override string ToString()
		{
			return $"{Name}({Number}): {Text}";
		}

		public static implicit operator int(ZSymbol errnum)
		{
			return errnum.Number;
		}

	}
}