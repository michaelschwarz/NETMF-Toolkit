using System;
using System.Collections.Generic;
using System.Text;

namespace MSchwarz.Net.NetBIOS
{
    public enum NbtType : ushort
    {
        /// <summary>
        /// NetBIOS general Name Service Resource Record
        /// </summary>
        NB = 32,

        /// <summary>
        /// NetBIOS NODE STATUS Resource Record
        /// </summary>
        NBSTAT = 33
    }
}
