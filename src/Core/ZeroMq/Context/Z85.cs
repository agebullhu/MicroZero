using System;
using System.Runtime.InteropServices;
using System.Text;
using ZeroMQ.lib;

namespace ZeroMQ
{
#pragma warning disable CS1591 // 缺少对公共可见类型或成员的 XML 注释
	public static class Z85
#pragma warning restore CS1591 // 缺少对公共可见类型或成员的 XML 注释
	{
#pragma warning disable CS1591 // 缺少对公共可见类型或成员的 XML 注释
		public static void CurveKeypair(out byte[] publicKey, out byte[] secretKey)
#pragma warning restore CS1591 // 缺少对公共可见类型或成员的 XML 注释
		{
			const int destLen = 40;
			using (var publicKeyData = MarshalPtr.Alloc(destLen + 1))
			using (var secretKeyData = MarshalPtr.Alloc(destLen + 1))
			{
				if (0 != zmq.curve_keypair(publicKeyData, secretKeyData))
				{
					throw new InvalidOperationException();
				}
				
				publicKey = new byte[destLen];
				Marshal.Copy(publicKeyData, publicKey, 0, destLen);
				
				secretKey = new byte[destLen];
				Marshal.Copy(secretKeyData, secretKey, 0, destLen);
			}
		}

#pragma warning disable CS1591 // 缺少对公共可见类型或成员的 XML 注释
		public static byte[] Encode(byte[] decoded)
#pragma warning restore CS1591 // 缺少对公共可见类型或成员的 XML 注释
		{
			var dataLen = decoded.Length;
			if (dataLen % 4 > 0)
			{
				throw new InvalidOperationException("decoded.Length must be divisible by 4");
			}
			var destLen = (Int32)(decoded.Length * 1.25);

			var data = GCHandle.Alloc(decoded, GCHandleType.Pinned);

            // the buffer dest must be one byte larger than destLen to accomodate the null termination character
			using (var dest = MarshalPtr.Alloc(destLen + 1))
			{
				if (IntPtr.Zero == zmq.z85_encode(dest, data.AddrOfPinnedObject(), dataLen))
				{
					data.Free();
					throw new InvalidOperationException();
				}
				data.Free();

				var bytes = new byte[destLen];

				Marshal.Copy(dest, bytes, 0, destLen);

				return bytes;
			}
		}

#pragma warning disable CS1591 // 缺少对公共可见类型或成员的 XML 注释
		public static byte[] ToZ85Encoded(this byte[] decoded) 
#pragma warning restore CS1591 // 缺少对公共可见类型或成员的 XML 注释
		{
			return Encode(decoded);
		}

#pragma warning disable CS1591 // 缺少对公共可见类型或成员的 XML 注释
		public static string ToZ85Encoded(this string decoded) 
#pragma warning restore CS1591 // 缺少对公共可见类型或成员的 XML 注释
		{
			return Encode(decoded, ZContext.Encoding);
		}

#pragma warning disable CS1591 // 缺少对公共可见类型或成员的 XML 注释
		public static string ToZ85Encoded(this string decoded, Encoding encoding) 
#pragma warning restore CS1591 // 缺少对公共可见类型或成员的 XML 注释
		{
			return Encode(decoded, encoding);
		}

#pragma warning disable CS1591 // 缺少对公共可见类型或成员的 XML 注释
		public static byte[] ToZ85EncodedBytes(this string decoded) 
#pragma warning restore CS1591 // 缺少对公共可见类型或成员的 XML 注释
		{
			return EncodeBytes(decoded, ZContext.Encoding);
		}

#pragma warning disable CS1591 // 缺少对公共可见类型或成员的 XML 注释
		public static byte[] ToZ85EncodedBytes(this string decoded, Encoding encoding) 
#pragma warning restore CS1591 // 缺少对公共可见类型或成员的 XML 注释
		{
			return EncodeBytes(decoded, encoding);
		}

#pragma warning disable CS1591 // 缺少对公共可见类型或成员的 XML 注释
		public static string Encode(string strg)
#pragma warning restore CS1591 // 缺少对公共可见类型或成员的 XML 注释
		{
			return Encode(strg, ZContext.Encoding);
		}

#pragma warning disable CS1591 // 缺少对公共可见类型或成员的 XML 注释
		public static string Encode(string strg, Encoding encoding)
#pragma warning restore CS1591 // 缺少对公共可见类型或成员的 XML 注释
		{
			var encoded = EncodeBytes(strg, encoding);
			return encoding.GetString(encoded);
		}

#pragma warning disable CS1591 // 缺少对公共可见类型或成员的 XML 注释
		public static byte[] EncodeBytes(string strg)
#pragma warning restore CS1591 // 缺少对公共可见类型或成员的 XML 注释
		{
			return EncodeBytes(strg, ZContext.Encoding);
		}

#pragma warning disable CS1591 // 缺少对公共可见类型或成员的 XML 注释
		public static byte[] EncodeBytes(string strg, Encoding encoding)
#pragma warning restore CS1591 // 缺少对公共可见类型或成员的 XML 注释
		{
			var bytes = encoding.GetBytes(strg);
			return Encode(bytes);
		}


#pragma warning disable CS1591 // 缺少对公共可见类型或成员的 XML 注释
		public static byte[] Decode(byte[] encoded)
#pragma warning restore CS1591 // 缺少对公共可见类型或成员的 XML 注释
		{
			var dataLen = encoded.Length;
			if (dataLen % 5 > 0)
			{
				throw new InvalidOperationException("encoded.Length must be divisible by 5");
			}
			var destLen = (Int32)(encoded.Length * .8);

			var data = GCHandle.Alloc(encoded, GCHandleType.Pinned);

			using (var dest = MarshalPtr.Alloc(destLen))
			{
				if (IntPtr.Zero == zmq.z85_decode(dest, data.AddrOfPinnedObject()))
				{
					data.Free();
					throw new InvalidOperationException();
				}
				data.Free();

				var decoded = new byte[destLen];

				Marshal.Copy(dest, decoded, 0, decoded.Length);

				return decoded;
			}
		}

#pragma warning disable CS1591 // 缺少对公共可见类型或成员的 XML 注释
		public static byte[] ToZ85Decoded(this byte[] encoded) 
#pragma warning restore CS1591 // 缺少对公共可见类型或成员的 XML 注释
		{
			return Decode(encoded);
		}

#pragma warning disable CS1591 // 缺少对公共可见类型或成员的 XML 注释
		public static string ToZ85Decoded(this string encoded) 
#pragma warning restore CS1591 // 缺少对公共可见类型或成员的 XML 注释
		{
			return Decode(encoded, ZContext.Encoding);
		}

#pragma warning disable CS1591 // 缺少对公共可见类型或成员的 XML 注释
		public static string ToZ85Decoded(this string encoded, Encoding encoding) 
#pragma warning restore CS1591 // 缺少对公共可见类型或成员的 XML 注释
		{
			return Decode(encoded, encoding);
		}

#pragma warning disable CS1591 // 缺少对公共可见类型或成员的 XML 注释
		public static byte[] ToZ85DecodedBytes(this string encoded) 
#pragma warning restore CS1591 // 缺少对公共可见类型或成员的 XML 注释
		{
			return DecodeBytes(encoded, ZContext.Encoding);
		}

#pragma warning disable CS1591 // 缺少对公共可见类型或成员的 XML 注释
		public static byte[] ToZ85DecodedBytes(this string encoded, Encoding encoding) 
#pragma warning restore CS1591 // 缺少对公共可见类型或成员的 XML 注释
		{
			return DecodeBytes(encoded, encoding);
		}

#pragma warning disable CS1591 // 缺少对公共可见类型或成员的 XML 注释
		public static string Decode(string strg)
#pragma warning restore CS1591 // 缺少对公共可见类型或成员的 XML 注释
		{
			return Decode(strg, ZContext.Encoding);
		}

#pragma warning disable CS1591 // 缺少对公共可见类型或成员的 XML 注释
		public static string Decode(string strg, Encoding encoding)
#pragma warning restore CS1591 // 缺少对公共可见类型或成员的 XML 注释
		{
			var encoded = DecodeBytes(strg, encoding);
			return encoding.GetString(encoded);
		}
		
#pragma warning disable CS1591 // 缺少对公共可见类型或成员的 XML 注释
		public static byte[] DecodeBytes(string strg, Encoding encoding)
#pragma warning restore CS1591 // 缺少对公共可见类型或成员的 XML 注释
		{
			var bytes = encoding.GetBytes(strg);
			return Decode(bytes);
		}
	}
}

