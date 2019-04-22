using System.Text;
using Newtonsoft.Json;

namespace Agebull.MicroZero
{
    /// <summary>
    /// Zmq基本扩展
    /// </summary>
    public static class ZmqExtend
    {
        private static readonly byte[] EmptyBytes = new byte[] { 0 };

        /// <summary>
        /// 转为UTF8字节
        /// </summary>
        /// <param name="word">单词</param>
        /// <returns>字节</returns>
        public static byte[] ToZeroBytes(this string word)
        {
            return string.IsNullOrEmpty(word) ? EmptyBytes : Encoding.UTF8.GetBytes(word);
        }
    }
}