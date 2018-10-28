using Agebull.ZeroNet.PubSub;

namespace Gboxt.Common.DataModel.ZeroNet
{
    public class TestItem : IPublishData
    {
        public string Abc { get; set; }

        public string Title => "Test";
    }

    /// <summary>
    /// 实体事件代理,实现网络广播功能
    /// </summary>
    public class TestEventProxy : SignlePublisher<TestItem>
    {
        /// <summary>
        /// 防止构造
        /// </summary>
        TestEventProxy()
        {
            Name = "EntityEvent";
            StationName = "AuthEntityEvent";
        }

        public static TestEventProxy Instance { get; } = new TestEventProxy();
    }
}
