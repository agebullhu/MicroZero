

namespace Agebull.MicroZero.PubSub
{
    /// <summary>
    ///     消息发布
    /// </summary>
    internal class ZPublisher : IZeroPublisher
    {
        bool IZeroPublisher.Publish(string station, string title, string sub, string arg)
        {
            return ZeroPublisher.DoPublish(station, title, sub, arg);
        }
    }
}