using System.IO;

namespace CompressionLib.MultithreadedZip
{
    /// <summary>
    /// Exposes compression and decompression functionality
    /// </summary>
    public sealed class MultithreadedZip : IZip
    {
        /// <summary>
        /// Compress a file
        /// </summary>
        /// <param name="compressSourceFile">source file</param>
        /// <param name="compressDestinationFile">destination for the compressed file</param>
        public void Compress(FileInfo compressSourceFile, FileInfo compressDestinationFile = null)
        {
            //TODO: check destination or create new name
            using (var compressor = new Compressor(compressSourceFile, compressDestinationFile))
            {
                compressor.Start();
            }         
        }

        /// <summary>
        /// Decompress a file
        /// </summary>
        /// <param name="decompressSourceFile">Source of a compressed file</param>
        /// <param name="decompressDestinationFile">Destination of a decompressed file</param>
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
