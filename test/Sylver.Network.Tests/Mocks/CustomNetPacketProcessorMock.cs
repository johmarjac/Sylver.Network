using Moq;
using Sylver.Network.Data;
using Sylver.Network.Data.Internal;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Sylver.Network.Tests.Mocks
{
    internal sealed class CustomNetPacketProcessor : IPacketProcessor
    {
        public int HeaderSize => sizeof(long);

        public bool IncludeHeader { get; }

        public CustomNetPacketProcessor(bool includeHeader)
        {
            IncludeHeader = includeHeader;
        }

        public int GetMessageLength(byte[] buffer)
        {
            return BitConverter.ToInt32(BitConverter.IsLittleEndian
                ? buffer.Take(HeaderSize).ToArray()
                : buffer.Take(HeaderSize).Reverse().ToArray(), 0);
        }

        public INetPacketStream CreatePacket(byte[] buffer) => new NetPacket(buffer);

        /// <inheritdoc />
        public IEnumerable<byte[]> ParseIncomingData(NetDataToken token, byte[] buffer, int bytesTransfered)
        {
            var messages = new List<byte[]>();

            while (token.DataStartOffset < bytesTransfered)
            {
                int headerSize = HeaderSize;

                if (token.ReceivedHeaderBytesCount < headerSize)
                {
                    if (token.HeaderData == null)
                    {
                        token.HeaderData = new byte[headerSize];
                    }

                    int bufferRemainingBytes = bytesTransfered - token.DataStartOffset;
                    int headerRemainingBytes = headerSize - token.ReceivedHeaderBytesCount;
                    int bytesToRead = Math.Min(bufferRemainingBytes, headerRemainingBytes);

                    Buffer.BlockCopy(buffer, token.DataStartOffset, token.HeaderData, token.ReceivedHeaderBytesCount, bytesToRead);

                    token.ReceivedHeaderBytesCount += bytesToRead;
                    token.DataStartOffset += bytesToRead;
                }

                if (token.ReceivedHeaderBytesCount == headerSize && token.HeaderData != null)
                {
                    if (!token.MessageSize.HasValue)
                    {
                        token.MessageSize = GetMessageLength(token.HeaderData);
                    }

                    if (token.MessageSize.Value < 0)
                    {
                        throw new InvalidOperationException("Message size cannot be smaller than zero.");
                    }

                    if (token.MessageData == null)
                    {
                        token.MessageData = new byte[token.MessageSize.Value];
                    }

                    if (token.ReceivedMessageBytesCount < token.MessageSize.Value)
                    {
                        int bufferRemainingBytes = bytesTransfered - token.DataStartOffset;
                        int messageRemainingBytes = token.MessageSize.Value - token.ReceivedMessageBytesCount;
                        int bytesToRead = Math.Min(bufferRemainingBytes, messageRemainingBytes);

                        Buffer.BlockCopy(buffer, token.DataStartOffset, token.MessageData, token.ReceivedMessageBytesCount, bytesToRead);

                        token.ReceivedMessageBytesCount += bytesToRead;
                        token.DataStartOffset += bytesToRead;
                    }
                }

                if (token.IsMessageComplete)
                {
                    messages.Add(BuildClientMessageData(token));
                    token.Reset();
                }
            }

            token.DataStartOffset = 0;

            return messages;
        }



        /// <summary>
        /// Builds the received message data based on the given data token.
        /// </summary>
        /// <param name="token">Client data token.</param>
        /// <returns>Client received data.</returns>
        private byte[] BuildClientMessageData(NetDataToken token)
        {
            var bufferSize = IncludeHeader ? HeaderSize + token.MessageSize.Value : token.MessageSize.Value;
            var buffer = new byte[bufferSize];

            if (IncludeHeader)
            {
                Array.Copy(token.HeaderData, 0, buffer, 0, HeaderSize);
                Array.Copy(token.MessageData, 0, buffer, HeaderSize, token.MessageSize.Value);
            }
            else
            {
                Array.Copy(token.MessageData, 0, buffer, 0, token.MessageSize.Value);
            }

            return buffer;
        }

    }

    internal sealed class CustomNetPacketProcessorMock : Mock<CustomNetPacketProcessor>
    {
    }
}
