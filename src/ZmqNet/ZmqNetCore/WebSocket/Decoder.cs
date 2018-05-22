using System;
using System.Diagnostics;

namespace NetMQ.WebSockets
{
    enum OpcodeEnum : byte
    {
        Continuation = 0,
        Text = 0x01,
        Binary = 0x02,
        Close = 0x08,
        Ping = 0x09,
        Pong = 0xA
    }

    class MessageEventArgs : EventArgs
    {
        public MessageEventArgs(OpcodeEnum opcode, byte[] payload, bool more)
        {
            Opcode = opcode;
            Payload = payload;
            More = more;
        }

        public OpcodeEnum Opcode { get; private set; }
        public byte[] Payload { get; private set; }
        public bool More { get; private set; }
    }

    class Decoder
    {
        const byte FinalBit = 0x80;
        private const byte MaskedBit = 0x80;

        enum State
        {
            NewMessage,
            SecondByte,
            ShortSize,
            ShortSize2,
            LongSize,
            LongSize2,
            LongSize3,
            LongSize4,
            LongSize5,
            LongSize6,
            LongSize7,
            LongSize8,
            Mask,
            Mask2,
            Mask3,
            Mask4,
            MoreByte,
            Payload,
            PayloadInProgress,
        }

        private State m_state;

        private bool m_final;
        private OpcodeEnum m_opcode;
        private bool m_isMaksed;
        private byte[] m_mask = new byte[4];

        private bool m_more;
        private int m_payloadLength;
        private byte[] m_payload;
        private int m_payloadIndex;

        public event EventHandler<MessageEventArgs> Message;

        public void Process(byte[] message)
        {
            int i = 0;

            while (i < message.Length)
            {
                switch (m_state)
                {
                    case State.Payload:
                        m_payloadIndex = 0;
                        m_payload = new byte[m_payloadLength];
                        goto case State.PayloadInProgress;

                    case State.PayloadInProgress:
                        int bytesToRead = m_payloadLength - m_payloadIndex;

                        if (bytesToRead > (message.Length - i))
                        {
                            bytesToRead = message.Length - i;
                        }

                        Buffer.BlockCopy(message, i, m_payload, m_payloadIndex, bytesToRead);

                        if (m_isMaksed)
                        {
                            for (int j = m_payloadIndex; j < m_payloadIndex + bytesToRead; j++)
                            {
                                if (m_opcode == OpcodeEnum.Binary)
                                {
                                    // because the first byte is the more bit we always add + 1 to the index 
                                    // when retrieving the mask byte
                                    m_payload[j] = (byte) (m_payload[j] ^ m_mask[(j + 1) % 4]);
                                }
                                else
                                {
                                    m_payload[j] = (byte) (m_payload[j] ^ m_mask[(j) % 4]);
                                }
                            }
                        }

                        m_payloadIndex += bytesToRead;
                        i += bytesToRead;

                        if (m_payloadIndex < m_payloadLength)
                        {
                            m_state = State.PayloadInProgress;
                        }
                        else
                        {
                            Message?.Invoke(this, new MessageEventArgs(m_opcode, m_payload, m_more));
                            m_state = State.NewMessage;
                        }

                        break;
                    default:
                        Process(message[i]);
                        i++;
                        break;
                }
            }
        }

        State NextState()
        {
            if ((m_state == State.LongSize8 || m_state == State.SecondByte || m_state == State.ShortSize2) &&
                m_isMaksed)
            {
                return State.Mask;
            }
            else if (m_opcode == OpcodeEnum.Binary)
            {
                return State.MoreByte;
            }
            else
            {
                return State.Payload;
            }
        }

        void Process(byte b)
        {
            switch (m_state)
            {
                case State.NewMessage:
                    m_final = (b & FinalBit) != 0;
                    m_opcode = (OpcodeEnum) (b & 0xF);
                    m_state = State.SecondByte;
                    break;
                case State.SecondByte:
                    m_isMaksed = (b & MaskedBit) != 0;
                    byte length = (byte) (b & 0x7F);

                    if (length < 126)
                    {
                        m_payloadLength = length;
                        m_state = NextState();
                    }
                    else if (length == 126)
                    {
                        m_state = State.ShortSize;
                    }
                    else
                    {
                        m_state = State.LongSize;
                    }

                    break;
                case State.Mask:
                    m_mask[0] = b;
                    m_state = State.Mask2;
                    break;
                case State.Mask2:
                    m_mask[1] = b;
                    m_state = State.Mask3;
                    break;
                case State.Mask3:
                    m_mask[2] = b;
                    m_state = State.Mask4;
                    break;
                case State.Mask4:
                    m_mask[3] = b;
                    m_state = NextState();
                    break;
                case State.ShortSize:
                    m_payloadLength = b << 8;
                    m_state = State.ShortSize2;
                    break;
                case State.ShortSize2:
                    m_payloadLength |= b;
                    m_state = NextState();
                    break;
                case State.LongSize:
                    m_payloadLength = 0;

                    // must be zero, max message size is MaxInt
                    Debug.Assert(b == 0);
                    m_state = State.LongSize2;
                    break;
                case State.LongSize2:
                    // must be zero, max message size is MaxInt
                    Debug.Assert(b == 0);
                    m_state = State.LongSize3;
                    break;
                case State.LongSize3:
                    // must be zero, max message size is MaxInt
                    Debug.Assert(b == 0);
                    m_state = State.LongSize4;
                    break;
                case State.LongSize4:
                    // must be zero, max message size is MaxInt
                    Debug.Assert(b == 0);
                    m_state = State.LongSize5;
                    break;
                case State.LongSize5:
                    m_payloadLength |= b << 24;
                    m_state = State.LongSize6;
                    break;
                case State.LongSize6:
                    m_payloadLength |= b << 16;
                    m_state = State.LongSize7;
                    break;
                case State.LongSize7:
                    m_payloadLength |= b << 8;
                    m_state = State.LongSize8;
                    break;
                case State.LongSize8:
                    m_payloadLength |= b;
                    m_state = NextState();
                    break;
                case State.MoreByte:
                    // The first byte of the payload is the more bit                  

                    if (m_isMaksed)
                    {
                        m_more = (b ^ m_mask[0]) == 1;
                    }
                    else
                    {
                        m_more = b == 1;
                    }

                    m_payloadLength--;

                    m_state = State.Payload;
                    break;
            }
        }
    }
}