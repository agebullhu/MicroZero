using System.Text;
using Newtonsoft.Json;

namespace Agebull.ZeroNet.Core
{
    /// <summary>
    /// Zmq基本扩展
    /// </summary>
    public static class ZmqExtend
    {
        private static readonly byte[] EmptyBytes = Encoding.UTF8.GetBytes("");

        /// <summary>
        /// 转为UTF8字节
        /// </summary>
        /// <param name="word">单词</param>
        /// <returns>字节</returns>
        public static byte[] ToZeroBytes(this string word)
        {
            return string.IsNullOrEmpty(word) ? EmptyBytes : Encoding.UTF8.GetBytes(word);
        }

        /// <summary>
        /// 转为UTF8字节
        /// </summary>
        /// <param name="v"></param>
        /// <returns>字节</returns>
        public static byte[] ToZeroBytes<T>(this T v) where T : class
        {
            return v == null ? EmptyBytes : Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(v));
        }

    }
}