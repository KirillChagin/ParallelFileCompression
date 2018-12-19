using System.IO;

namespace CompressionLib.MultithreadedZip
{
    public sealed class MultithreadedZip : IZip
    {
        public void Compress(FileInfo compressSourceFile, FileInfo compressDestinationFile = null)
        {
            //TODO: check destination or create new name
            using (var compressor = new Compressor(compressSourceFile, compressDestinationFile))
            {
                compressor.Start();
            }         
        }

        public void Decompress(FileInfo decompressSourceFile, FileInfo decompressDestinationFile = null)
        {
            //TODO: check destination or create new name
            using (var decompressor = new Decompressor(decompressSourceFile, decompressDestinationFile))
            {
                decompressor.Start();
            }            
        }
    }
}
