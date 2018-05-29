using System.Collections.Generic;

namespace NetMQ.WebSockets
{
    /// <summary>
    /// WebSocket发布器
    /// </summary>
    public class WsPublisher : WsSocket
    {

        /// <summary>
        /// 构造
        /// </summary>
        public WsPublisher() : base(id => new PublisherShimHandler(id))
        {
        }
        /// <summary>
        /// 发布垫片
        /// </summary>
        private class PublisherShimHandler : BaseShimHandler
        {
            //  List of all subscriptions mapped to corresponding pipes.
            private readonly Mtrie _subscriptions;

            private static readonly Mtrie.MtrieDelegate MarkAsMatching;
            private static readonly Mtrie.MtrieDelegate SendUnsubscription;
            private static readonly ByteArrayEqualityComparer ByteArrayEqualityComparer;

            private readonly List<byte[]> _identities;
            private int _matching = 0;

            static PublisherShimHandler()
            {
                MarkAsMatching = (pipe, data, arg) =>
                {
                    PublisherShimHandler self = (PublisherShimHandler) arg;

                    self._identities.Swap(IndexOf(self._identities, pipe), self._matching);
                    self._matching++;
                };

                SendUnsubscription = (pipe, data, arg) => { };

                ByteArrayEqualityComparer = new ByteArrayEqualityComparer();
            }

            private static int IndexOf(IList<byte[]> list, byte[] identity)
            {
                for (int i = 0; i < list.Count; i++)
                {
                    if (ByteArrayEqualityComparer.Equals(list[i], identity))
                    {
                        return i;
                    }
                }

                return -1;
            }


            public PublisherShimHandler(int id)
                : base(id)
            {
                _identities = new List<byte[]>();
                _subscriptions = new Mtrie();
            }

            protected override void OnOutgoingMessage(NetMQMessage message)
            {
                _subscriptions.Match(message[0].ToByteArray(false), message[0].MessageSize, MarkAsMatching, this);

                while (message.FrameCount > 0)
                {
                    var frame = message.Pop().ToByteArray();
                    bool more = message.FrameCount > 0;

                    for (int i = 0; i < _matching; i++)
                    {
                        WriteOutgoing(_identities[i], frame, more);
                    }
                }

                _matching = 0;
            }

            protected override void OnIncomingMessage(byte[] identity, NetMQMessage message)
            {
                byte[] data = message.Pop().ToByteArray();

                if (data.Length > 0 && (data[0] == 1 || data[0] == 0))
                {
                    if (data[0] == 0)
                    {
                        _subscriptions.Remove(data, 1, identity);
                    }
                    else
                    {
                        _subscriptions.Add(data, 1, identity);
                    }
                }
            }

            protected override void OnNewClient(byte[] identity)
            {
                _identities.Add(identity);
            }

            protected override void OnClientRemoved(byte[] identity)
            {
                _subscriptions.RemoveHelper(identity, SendUnsubscription, this);

                int index = IndexOf(_identities, identity);

                if (index < _matching)
                {
                    _matching--;
                }

                _identities.Remove(identity);
            }
        }
    }
}