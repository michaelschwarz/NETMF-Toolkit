using System;

namespace MFToolkit.Net.XBee
{
    public enum TxStatusType : byte
    {
        Success = 0x00,
        NoAcknowledgementReceived = 0x01,
        CCAFailure = 0x02,
        Purged = 0x03
    }
}
