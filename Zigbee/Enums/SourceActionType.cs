using System;

namespace MFToolkit.Net.XBee
{
    // TODO: the values for this enum are maybe not defined correct, documentation says 1 and 2, but I get 3 when joining
    public enum SourceActionType : byte
    {
        /// <summary>
        /// Unknown source action or AtCommend response.
        /// </summary>
        Unknown = 0x00,

        NodeIdentificationButton = 0x01,

        JoiningEvent = 0x03
    }
}
