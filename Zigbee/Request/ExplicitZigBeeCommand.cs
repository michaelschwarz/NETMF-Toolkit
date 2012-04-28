using System;
using System.Text;
using MFToolkit.IO;

namespace MFToolkit.Net.XBee
{
    public class ExplicitZigBeeCommand : XBeeFrameRequest
    {
        /// <summary>
        /// Set to the 64-bit address of the destination device. The
        /// following addresses are also supported:
        /// 0x0000000000000000 - Reserved 64-bit address for the coordinator
        /// 0x000000000000FFFF - Broadcast address
        /// </summary>
        public XBeeAddress64 DestinationAddress64 { get; set; }

        /// <summary>
        /// Set to the 16-bit address of the destination device, if
        /// known. Set to 0xFFFE if the address is unknown, or if
        /// sending a broadcast.
        /// </summary>
        public XBeeAddress16 DestinationAddress16 { get; set; }

        /// <summary>
        /// Source endpoint for the transmission
        /// </summary>
        public byte SourceEndpoint { get; set; }

        /// <summary>
        /// Destination endpoint for the transmission.
        /// </summary>
        public byte DestinationEndpoint { get; set; }

        /// <summary>
        /// Cluster ID used in the transmission
        /// </summary>
        public ushort ClusterId { get; set; }

        /// <summary>
        /// Profile ID used in the transmission
        /// </summary>
        public ushort ProfileId { get; set; }

        /// <summary>
        /// Sets the maximum number of hops a broadcast
        /// transmission can traverse. If set to 0, the transmission
        /// radius will be set to the network maximum hops value.
        /// </summary>
        public byte BroadcastRadius { get; set; }

        /// <summary>
        /// Bitfield of supported transmission options. Supported
        /// values include the following:
        /// 0x20 - Enable APS encryption (if EE=1)
        /// 0x40 - Use the extended transmission timeout for this destination
        /// 
        /// Enabling APS encryption decreases the maximum
        /// number of RF payload bytes by 4 (below the value
        /// reported by NP).
        /// 
        /// Setting the extended timeout bit causes the stack to set
        /// the extended transmission timeout for the destination
        /// address. (See chapter 4.)
        /// 
        /// All unused and unsupported bits must be set to 0.
        /// </summary>
        public byte Options { get; set; }

        public byte[] Payload { get; set; }


        public ExplicitZigBeeCommand()
            : base(XBeeApiType.ExplicitAddressingZigBeeCommandFrame)
        {
        }

        public ExplicitZigBeeCommand(XBeeAddress64 address64, XBeeAddress16 address16, byte sourceEndpoint, byte destinationEndpoint, ushort clusterId, ushort profileId, byte[] payload)
                    : base(XBeeApiType.ExplicitAddressingZigBeeCommandFrame)
        {
            DestinationAddress64 = address64;
            DestinationAddress16 = address16;
            SourceEndpoint = sourceEndpoint;
            DestinationEndpoint = destinationEndpoint;
            ClusterId = clusterId;
            ProfileId = profileId;
            Payload = payload;
        }

        internal override void WriteApiBytes(ByteWriter bw)
        {
            base.WriteApiBytes(bw);

            DestinationAddress64.WriteBytes(bw);
            DestinationAddress16.WriteBytes(bw);

            bw.Write(SourceEndpoint);
            bw.Write(DestinationEndpoint);
            bw.Write((ushort)ClusterId);
            bw.Write((ushort)ProfileId);
            bw.Write(BroadcastRadius);
            bw.Write(Options);
            bw.Write(Payload);
        }
    }
}
