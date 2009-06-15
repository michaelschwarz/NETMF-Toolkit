using System;

namespace MFToolkit.Net.XBee
{
    public interface IAtCommandResponse
    {
        string Command { get; }
        byte[] Value { get; }
    }
}
