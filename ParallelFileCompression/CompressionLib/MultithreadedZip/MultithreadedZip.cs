using System.IO;

namespace CompressionLib.MultithreadedZip
{
    public sealed class MultithreadedZip : IZip
    {
        public void Compress(FileInfo compressSourceFile, FileInfo compressDestinationFile = null)
        {
            var compressor = new Compressor(compressSourceFile, compressDestinationFile);
            compressor.Execute();
        }

        public void Decompress(FileInfo decompressSourceFile, FileInfo decompressDestinationFile = null)
        {
            var decompressor = new Decompressor(decompressSourceFile, decompressDestinationFile);
            decompressor.Execute();
        }
    }
}
