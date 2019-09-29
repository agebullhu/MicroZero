using System;
using Microsoft.AspNetCore.Http;

namespace MicroZero.Http.Gateway
{

    /// <summary>
    ///     调用映射核心类
    /// </summary>
    public interface IRouter
    {
        /// <summary>
        ///     开始
        /// </summary>
        bool Prepare(HttpContext context);

        /// <summary>
        ///     调用
        /// </summary>
        void Call();

        /// <summary>
        ///     写入返回
        /// </summary>
        void WriteResult();

        /// <summary>
        ///     结束
        /// </summary>
        void End();

        /// <summary>
        /// 异常处理
        /// </summary>
        /// <param name="e"></param>
        /// <param name="context"></param>
        void OnError(Exception e, HttpContext context);

        /// <summary>
        /// 检查并重新映射（如果可以的话）
        /// </summary>
        /// <param name="map"></param>
        void CheckMap(AshxMapConfig map);
    }
}