using System.Collections.Generic;

namespace NetMQ.WebSockets
{
    public class WSPublisher : WsSocket
    {
        private class PublisherShimHandler : BaseShimHandler
        {
            //  List of all subscriptions mapped to corresponding pipes.
            private readonly Mtrie m_subscriptions;

            private static readonly Mtrie.MtrieDelegate s_markAsMatching;
            private static readonly Mtrie.MtrieDelegate s_SendUnsubscription;
            private static readonly ByteArrayEqualityComparer s_byteArrayEqualityComparer;

            private List<byte[]> m_identities;
            private int m_matching = 0;

            static PublisherShimHandler()
            {
                s_markAsMatching = (pipe, data, arg) =>
                {
                    PublisherShimHandler self = (PublisherShimHandler) arg;

                    self.m_identities.Swap(IndexOf(self.m_identities, pipe), self.m_matching);
                    self.m_matching++;
                };

                s_SendUnsubscription = (pipe, data, arg) => { };

                s_byteArrayEqualityComparer = new ByteArrayEqualityComparer();
            }

            private static int IndexOf(IList<byte[]> list, byte[] identity)
            {
                for (int i = 0; i < list.Count; i++)
                {
                    if (s_byteArrayEqualityComparer.Equals(list[i], identity))
                    {
                        return i;
                    }
                }

                return -1;
            }


            public PublisherShimHandler(int id)
                : base(id)
            {
                m_identities = new List<byte[]>();
                m_subscriptions = new Mtrie();
            }

            protected override void OnOutgoingMessage(NetMQMessage message)
            {
                m_subscriptions.Match(message[0].ToByteArray(false), message[0].MessageSize, s_markAsMatching, this);

                while (message.FrameCount > 0)
                {
                    var frame = message.Pop().ToByteArray();
                    bool more = message.FrameCount > 0;

                    for (int i = 0; i < m_matching; i++)
                    {
                        WriteOutgoing(m_identities[i], frame, more);
                    }
                }

                m_matching = 0;
            }

            protected override void OnIncomingMessage(byte[] identity, NetMQMessage message)
            {
                byte[] data = message.Pop().ToByteArray();

                if (data.Length > 0 && (data[0] == 1 || data[0] == 0))
                {
                    if (data[0] == 0)
                    {
                        m_subscriptions.Remove(data, 1, identity);
                    }
                    else
                    {
                        m_subscriptions.Add(data, 1, identity);
                    }
                }
            }

            protected override void OnNewClient(byte[] identity)
            {
                m_identities.Add(identity);
            }

            protected override void OnClientRemoved(byte[] identity)
            {
                m_subscriptions.RemoveHelper(identity, s_SendUnsubscription, this);

                int index = IndexOf(m_identities, identity);

                if (index < m_matching)
                {
                    m_matching--;
                }

                m_identities.Remove(identity);
            }
        }

        public WSPublisher() : base(id => new PublisherShimHandler(id))
        {
        }
    }
}