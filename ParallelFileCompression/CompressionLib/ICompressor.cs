using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace CompressionLib
{
    public interface ICompressor
    {
        void Compress(FileInfo fileToCompress, FileInfo compressDestinationFile = null);

        void Decompress(FileInfo fileToDecompress, FileInfo decompressDestinationFile = null);
    }
}
