using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CompressionLib.MultithreadedZip
{
    internal class ByteBlock
    {
        public byte[] Buffer { get; }
        public int BlockNumber { get; }

        public ByteBlock(byte[] buffer, int blockNumber)
        {
            Buffer = buffer;
            BlockNumber = blockNumber;
        }
    }
}
