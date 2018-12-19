using System;

namespace CompressionLib
{
    internal static class ZipUtils
    {
        public static readonly int BufferSize = 1 * 1024 * 1024;

        public static readonly int BlockHeaderSize = sizeof(int);

        public static readonly int FileHeaderSize = sizeof(long) + 2 * sizeof(int); //file length, block length, last block length

        public static readonly string ZipExtension = ".gz";

        //TODO: better move to another class
        public static byte[] AddByteBlockLengthHeader(this byte[] array)
        {
            var arrayWithHeader = new byte[array.Length + BlockHeaderSize];

            BitConverter.GetBytes(array.Length).CopyTo(arrayWithHeader, 0);
            array.CopyTo(arrayWithHeader, sizeof(Int32));

            return arrayWithHeader;
        }
    }
}
