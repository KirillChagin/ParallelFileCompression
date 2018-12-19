using System.IO;

namespace CompressionLib
{
    /// <summary>
    /// Exposes a compress and decompress functionality
    /// </summary>
    public interface IZip
    {
        /// <summary>
        /// Compress a file
        /// </summary>
        /// <param name="compressSourceFile">source file</param>
        /// <param name="compressDestinationFile">destination for the compressed file</param>
        void Compress(FileInfo compressSourceFile, FileInfo compressDestinationFile = null);

        /// <summary>
        /// Decompress a file
        /// </summary>
        /// <param name="decompressSourceFile">Source of a compressed file</param>
        /// <param name="decompressDestinationFile">Destination of a decompressed file</param>
        void Decompress(FileInfo decompressSourceFile, FileInfo decompressDestinationFile = null);
    }
}
