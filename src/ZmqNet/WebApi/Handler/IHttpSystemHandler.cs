#if !NETSTANDARD2_0
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

namespace Agebull.ZeroNet.ZeroApi.WebApi
{
    /// <summary>
    /// 系统的HTTP处理接口
    /// </summary>
    public interface IHttpSystemHandler
    {
        /// <summary>
        /// 开始时的处理
        /// </summary>
        /// <param name="cancellationToken"></param>
        /// <param name="request"></param>
        /// <returns>如果返回内容不为空，直接返回,后续的处理不再继续</returns>
        Task<HttpResponseMessage> OnBegin(HttpRequestMessage request, CancellationToken cancellationToken);

        /// <summary>
        /// 结束时的处理
        /// </summary>
        /// <param name="cancellationToken"></param>
        /// <param name="request"></param>
        /// <param name="response"></param>
        void OnEnd(HttpRequestMessage request, CancellationToken cancellationToken, HttpResponseMessage response);
    }
}
# endif
