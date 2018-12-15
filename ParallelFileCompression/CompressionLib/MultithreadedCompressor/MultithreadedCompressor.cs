using System;
using System.IO;

namespace CompressionLib.MultithreadedCompressor
{
    public  class MultithreadedCompressor : ICompressor
    {
        public void Compress(FileInfo fileToCompress, FileInfo compressDestinationFile = null)
        {
            throw new NotImplementedException();
        }

        public void Decompress(FileInfo fileToDecompress, FileInfo decompressDestinationFile = null)
        {
            throw new NotImplementedException();
        }
    }
}
