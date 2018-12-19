namespace CompressionLib.MultithreadedZip
{
    internal class ByteBlock
    {
        public byte[] Buffer { get; }
        public int BlockNumber { get; }
        public bool IsLast { get; }

        public ByteBlock(byte[] buffer, int blockNumber, bool isLast = false)
        {
            Buffer = buffer;
            BlockNumber = blockNumber;
            IsLast = isLast;
        }
    }
}
