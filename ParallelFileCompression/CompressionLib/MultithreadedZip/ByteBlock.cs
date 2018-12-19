namespace CompressionLib.MultithreadedZip
{
    /// <summary>
    /// Wrapper class for a <code>byte[]</code> block
    /// </summary>
    internal class ByteBlock
    {
        /// <summary>
        /// Byte buffer
        /// </summary>
        public byte[] Buffer { get; }

        /// <summary>
        /// Ordinal number of a block
        /// </summary>
        public int BlockNumber { get; }

        /// <summary>
        /// Indicates that the current block is the last in a collection
        /// </summary>
        public bool IsLast { get; }

        /// <summary>
        /// Initializes a new instance of a <see cref="ByteBlock"/>
        /// </summary>
        /// <param name="buffer">Byte buffer</param>
        /// <param name="blockNumber">Ordinal number of a block</param>
        /// <param name="isLast">True if a block is the last in a collection</param>
        public ByteBlock(byte[] buffer, int blockNumber, bool isLast = false)
        {
            Buffer = buffer;
            BlockNumber = blockNumber;
            IsLast = isLast;
        }
    }
}
