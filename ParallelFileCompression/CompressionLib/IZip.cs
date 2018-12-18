using System.IO;

namespace CompressionLib
{
    public interface IZip
    {
        void Compress(FileInfo compressSourceFile, FileInfo compressDestinationFile = null);

        void Decompress(FileInfo decompressSourceFile, FileInfo decompressDestinationFile = null);
    }
}
