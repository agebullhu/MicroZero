using Agebull.MicroZero.PubSub;

namespace MicroZero.Http.Gateway
{
    /// <summary>
    /// 缓存事件处理器
    /// </summary>
    public class CacheEventProcess : SubStation
    {
        /// <summary>
        /// 构造
        /// </summary>
        public CacheEventProcess()
        {
            Name = "CacheEventProcess";
            StationName = "DataEvent";
            Subscribe = "Cache";
        }

        ///<inheritdoc/>
        public override void Handle(PublishItem args)
        {
            RouteCache.RemoveCache($"{args.SubTitle}?{args.Content}");
        }
    }
}