﻿using System.Collections.Generic;

namespace Sylver.Network.Data.Internal
{
    public class NetDataToken
    {
        /// <summary>
        /// Gets or sets the message header data.
        /// </summary>
        public byte[] HeaderData { get; set; }

        /// <summary>
        /// Gets or sets the number of bytes received for the the message header.
        /// </summary>
        public int ReceivedHeaderBytesCount { get; set; }

        /// <summary>
        /// Gets or sets the full message size.
        /// </summary>
        public int? MessageSize { get; set; }

        /// <summary>
        /// Gets or sets the number of bytes received for the message body.
        /// </summary>
        public int ReceivedMessageBytesCount { get; set; }

        /// <summary>
        /// Gets or sets the received message data.
        /// </summary>
        public byte[] MessageData;

        public List<byte> MessageData2;

        /// <summary>
        /// Gets or sets the data start offset.
        /// </summary>
        public int DataStartOffset { get; set; }

        /// <summary>
        /// Gets a value that indicates if the message is complete.
        /// </summary>
        public bool IsMessageComplete => MessageSize.HasValue ? ReceivedMessageBytesCount == MessageSize.Value : false;

        /// <summary>
        /// Reset the token data properties.
        /// </summary>
        public void Reset()
        {
            ReceivedHeaderBytesCount = 0;
            ReceivedMessageBytesCount = 0;
            HeaderData = null;
            MessageData = null;
            MessageSize = null;
            MessageData2 = null;
        }
    }
}
