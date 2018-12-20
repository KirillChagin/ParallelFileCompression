using System.IO;

namespace CompressionLib.MultithreadedZip
{
    /// <summary>
    /// Exposes compression and decompression functionality
    /// </summary>
    public sealed class MultithreadedZip : IZip
    {
        private ZipBase _processor;

        /// <summary>
        /// Compress a file
        /// </summary>
        /// <param name="compressSourceFile">source file</param>
        /// <param name="compressDestinationFile">destination for the compressed file</param>
        public void Compress(FileInfo compressSourceFile, FileInfo compressDestinationFile = null)
        {
            using (_processor = new Compressor(compressSourceFile, compressDestinationFile))
            {
                _processor.Start();
            }         
        }

        /// <summary>
        /// Decompress a file
        /// </summary>
        /// <param name="decompressSourceFile">Source of a compressed file</param>
        /// <param name="decompressDestinationFile">Destination of a decompressed file</param>
        public void Decompress(FileInfo decompressSourceFile, FileInfo decompressDestinationFile = null)
        {
            using (_processor = new Decompressor(decompressSourceFile, decompressDestinationFile))
            {
                _processor.Start();
            }            
        }

        /// <summary>
        /// Abort execution
        /// </summary>
        public void Abort()
        {
            _processor.Abort();
        }

        /// <summary>
        /// Indicates successful execution complete
        /// </summary>
        public bool Success => _processor.Success;

        /// <summary>
        /// Error message. Null if execution succeed
        /// </summary>
        public string ErrorMessage => _processor.ErrorMessage;
    }
}
