using System;
using Agebull.MicroZero.ZeroApis;
using System.Threading.Tasks;
using Agebull.MicroZero;
using Microsoft.AspNetCore.Mvc;
using Agebull.Common.Logging;
using System.Collections.Generic;

namespace ApiTest
{

    /// <summary>
    /// 测试
    /// </summary>
    public class TestController : ApiControlerEx
    {
        /// <summary>
        /// 处理文字请求
        /// </summary>
        /// <param name="Remark">说明</param>
        /// <returns></returns>
        [Route("v1/test"), HttpPost, ApiAccessOptionFilter(ApiAccessOption.Anymouse | ApiAccessOption.Public)]
        public Task<ApiResult<List<Semple>>> OnTextRequestA(List<Semple> Remark)
        {
            return Task.FromResult(ApiResult.Succees(Remark));
        }
    }
    /// <summary>
    /// 示例
    /// </summary>
    public class SempleItem
    {
        /// <summary>
        /// 文本
        /// </summary>
        public string Str { get; set; }
        /// <summary>
        /// DateTime
        /// </summary>
        public DateTime DateTime { get; set; }
        /// <summary>
        /// 文本
        /// </summary>
        public List<string> Values { get; set; }

    }
    /// <summary>
    /// 示例
    /// </summary>
    public class Semple
    {
        /// <summary>
        /// 文本
        /// </summary>
        public string Str { get; set; }
        /// <summary>
        /// 文本
        /// </summary>
        public List<SempleItem> Items { get; set; }
        /// <summary>
        /// DateTime
        /// </summary>
        public DateTime DateTime { get; set; }
        /// <summary>
        /// DateTime
        /// </summary>
        public double Double { get; set; }
        /// <summary>
        /// DateTime
        /// </summary>
        public float Float { get; set; }
        /// <summary>
        /// DateTime
        /// </summary>
        public decimal Decimal { get; set; }
        /// <summary>
        /// DateTime
        /// </summary>
        public long Long { get; set; }
        /// <summary>
        /// DateTime
        /// </summary>
        public int Int32 { get; set; }
        /// <summary>
        /// DateTime
        /// </summary>
        public uint Uint { get; set; }
        /// <summary>
        /// DateTime
        /// </summary>
        public short Short { get; set; }
        /// <summary>
        /// DateTime
        /// </summary>
        public byte Byte { get; set; }
        /// <summary>
        /// DateTime
        /// </summary>
        public char Char { get; set; }
    }
}
