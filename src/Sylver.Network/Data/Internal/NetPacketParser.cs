namespace Sylver.Network.Data.Internal
{
    /// <summary>
    /// Provides methods to parse incoming packets.
    /// </summary>
    internal class NetPacketParser
    {
        /// <summary>
        /// Gets or sets the packet processor.
        /// </summary>
        public IPacketProcessor PacketProcessor { get; set; }

        /// <summary>
        /// Creates a new <see cref="NetPacketParser"/> instance.
        /// </summary>
        /// <param name="packetProcessor">Net packet processor used to parse the incoming data.</param>
        public NetPacketParser(IPacketProcessor packetProcessor)
        {
            PacketProcessor = packetProcessor;
        }
    }
}
