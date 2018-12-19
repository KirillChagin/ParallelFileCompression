using System;

namespace CompressionLib
{
    internal static class ZipUtils
    {
        /// <summary>
        /// Size of the block for reading uncompressed file
        /// </summary>
        public static readonly int BufferSize = 1 * 1024 * 1024;

        /// <summary>
        /// Size of a header of a compressed block
        /// </summary>
        public static readonly int BlockHeaderSize = sizeof(int);

        /// <summary>
        /// Size of the compressed file's header
        /// </summary>
        public static readonly int FileHeaderSize = sizeof(long) + 2 * sizeof(int); //file length, block length, last block length

        /// <summary>
        /// Default file extension of a compressed file
        /// </summary>
        public static readonly string ZipExtension = ".gz";

        //TODO: better move to another class
        /// <summary>
        /// Creates a new <see cref="byte"/> with preceding bytes that indicates the length of the array
        /// </summary>
        /// <param name="array"></param>
        /// <returns></returns>
        public static byte[] AddByteBlockLengthHeader(this byte[] array)
        {
            var arrayWithHeader = new byte[array.Length + BlockHeaderSize];

            BitConverter.GetBytes(array.Length).CopyTo(arrayWithHeader, 0);
            array.CopyTo(arrayWithHeader, sizeof(Int32));

            return arrayWithHeader;
        }
    }
}
