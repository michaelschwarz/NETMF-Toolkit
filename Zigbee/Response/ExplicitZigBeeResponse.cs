using System;
using System.Text;
using MFToolkit.IO;

namespace MFToolkit.Net.XBee
{
    public class ExplicitZigBeeResponse : XBeeResponse
    {
        /// <summary>
        /// 64-bit address of sender. Set to 0xFFFFFFFFFFFFFFFF
        /// (unknown 64-bit address) if the sender's 64-bit address is
        /// unknown.
        /// </summary>
        public XBeeAddress64 SourceAddress64 { get; set; }

        /// <summary>
        /// 16-bit address of sender.
        /// </summary>
        public XBeeAddress16 SourceAddress16 { get; set; }

        /// <summary>
        /// Endpoint of the source that initiated the
        /// transmission
        /// </summary>
        public byte SourceEndpoint { get; set; }

        /// <summary>
        /// Endpoint of the destination the message is
        /// addressed to.
        /// </summary>
        public byte DestinationEndpoint { get; set; }

        /// <summary>
        /// Cluster ID the packet was addressed to.
        /// </summary>
        public ushort ClusterId { get; set; }

        /// <summary>
        /// Profile ID the packet was addressed to.
        /// </summary>
        public ushort ProfileId { get; set; }

        /// <summary>
        /// 0x01 – Packet Acknowledged
        /// 0x02 – Packet was a broadcast packet
        /// 0x20 - Packet encrypted with APS encryption
        /// 0x40 - Packet was sent from an end device (if known)
        /// </summary>
        public byte Options { get; set; }

        public byte[] Payload { get; set; }

        public ExplicitZigBeeResponse(short length, ByteReader br)
			: base(length, br)
		{
            SourceAddress64 = XBeeAddress64.ReadBytes(br);
            SourceAddress16 = XBeeAddress16.ReadBytes(br);
            SourceEndpoint = br.ReadByte();
            DestinationEndpoint = br.ReadByte();
            ClusterId = br.ReadUInt16();
            ProfileId = br.ReadUInt16();
            Options = br.ReadByte();
            Payload = br.GetAvailableBytes();
		}

    }
}
