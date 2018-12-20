using System;
using System.IO;
using System.IO.Compression;
using ZipThreading.CollectionProcessorThreadPool;

namespace CompressionLib.MultithreadedZip
{
    /// <summary>
    /// Exposes decompression functionality
    /// </summary>
    internal class Decompressor : ZipBase
    {
        /// <summary>
        /// Initializes a new instance of a decompressor
        /// </summary>
        /// <param name="sourceFile">Source file</param>
        /// <param name="destinationFile">Destination file</param>
        public Decompressor(FileInfo sourceFile, FileInfo destinationFile) : base(sourceFile, destinationFile)
        {
        }

        /// <summary>
        /// Decompression of a block method that is used as callback for <see cref="CollectionProcessorThreadPool{T}"/>
        /// </summary>
        /// <param name="block">Processing block</param>
        protected override void ProcessBlock(ByteBlock block)
        {
            if (block == null || IsAborted)
            {
                return;
            }

            using (var memoryStream = new MemoryStream(block.Buffer))
            {
                var decompressedBlock = block.IsLast ? new byte[OriginalLastBlockLength] : new byte[OriginalBlockSize];

                using (var gZipStream = new GZipStream(memoryStream, CompressionMode.Decompress))
                {
                    gZipStream.Read(decompressedBlock, 0, decompressedBlock.Length);
                }

                OutputBlocks.Add(block.BlockNumber, decompressedBlock);
            }
        }

        /// <summary>
        /// Reads of a source file
        /// </summary>
        /// <param name="fileInfo">Source file</param>
        protected override void ReadSource(FileInfo fileInfo)
        {
            using (var fileStream = new FileStream(fileInfo.FullName, FileMode.Open, FileAccess.Read, FileShare.Read))
            {
                var blocksRead = 0;
                long totalBytesRead = 0;
                var compressedFileSize = fileInfo.Length;

                ReadFileHeader(fileStream);
                totalBytesRead += ZipUtils.FileHeaderSize;

                while (!IsAborted)
                {
                    var blockHeader = new byte[ZipUtils.BlockHeaderSize];
                    var bytesRead = fileStream.Read(blockHeader, 0, ZipUtils.BlockHeaderSize);
                    totalBytesRead += bytesRead;

                    var blockLength = BitConverter.ToInt32(blockHeader, 0);
                    var block = new byte[blockLength];
                    bytesRead = fileStream.Read(block, 0, blockLength);
                    totalBytesRead += bytesRead;

                    if (compressedFileSize - totalBytesRead <= 0)
                    {
                        InputBlocks.Enqueue(new ByteBlock(block, blocksRead, true));
                        break;
                    }

                    InputBlocks.Enqueue(new ByteBlock(block, blocksRead));
                    blocksRead++;
                }

                InputBlocks.CompleteFilling();
            }
        }

        /// <summary>
        /// Writes to a destination file
        /// </summary>
        /// <param name="destinationFileInfo">Destination file</param>
        protected override void WriteToDestination(FileInfo destinationFileInfo)
        {
            using (var fileStream = new FileStream(destinationFileInfo.FullName, FileMode.Create))
            {
                var nextBlockNumber = 0;

                while (true && !IsAborted)
                {
                    var result = OutputBlocks.TryTakeAndRemove(nextBlockNumber, out var block, true);

                    if (!result)
                    {
                        break;
                    }

                    nextBlockNumber++;
                    fileStream.Write(block, 0, block.Length);
                }
            }
        }

        private void ReadFileHeader(FileStream fileStream)
        {
            var buffer = new byte[ZipUtils.FileHeaderSize];
            fileStream.Read(buffer, 0, buffer.Length);
            OriginalFileSize = BitConverter.ToInt64(buffer, 0);
            OriginalBlockSize = BitConverter.ToInt32(buffer, sizeof(long));
            OriginalLastBlockLength = BitConverter.ToInt32(buffer, sizeof(long) + sizeof(int));
        }
    }
}
