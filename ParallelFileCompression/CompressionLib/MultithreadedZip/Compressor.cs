using System;
using System.IO;
using System.IO.Compression;
using ZipThreading.CollectionProcessorThreadPool;

namespace CompressionLib.MultithreadedZip
{
    /// <summary>
    /// Exposes compression functionality
    /// </summary>
    internal class Compressor : ZipBase
    {
        /// <summary>
        /// Initializes a new instance of a compressor
        /// </summary>
        /// <param name="sourceFile">Source file</param>
        /// <param name="destinationFile">Destination file</param>
        public Compressor(FileInfo sourceFile, FileInfo destinationFile) : base(sourceFile, destinationFile)
        {
            OriginalBlockSize = ZipUtils.BufferSize;
            OriginalFileSize = sourceFile.Length;
            OriginalLastBlockLength = OriginalBlockSize;
        }

        /// <summary>
        /// Compression of a block method that is used as callback for <see cref="CollectionProcessorThreadPool{T}"/>
        /// </summary>
        /// <param name="block">Processing block</param>
        protected override void ProcessBlock(ByteBlock block)
        {
            if (block == null)
            {
                return; //TODO: throw?
            }

            try
            {
                using (var memoryStream = new MemoryStream())
                {
                    using (var gZipStream = new GZipStream(memoryStream, CompressionMode.Compress))
                    {
                        gZipStream.Write(block.Buffer, 0, block.Buffer.Length);
                    }

                    var compressedBlock = memoryStream.ToArray().AddByteBlockLengthHeader();
                    OutputBlocks.Add(block.BlockNumber, compressedBlock);
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                throw;
            }
            
        }

        /// <summary>
        /// Reads of a source file
        /// </summary>
        /// <param name="fileInfo">Source file</param>
        protected override void ReadSource(FileInfo fileInfo)
        {
            var buffer = new byte[OriginalBlockSize];

            try
            {
                using (var fileStream = new FileStream(fileInfo.FullName, FileMode.Open, FileAccess.Read, FileShare.Read))
                {
                    int bytesRead;
                    var blocksRead = 0;
                    while ((bytesRead = fileStream.Read(buffer, 0, buffer.Length)) > 0)
                    {
                        if (bytesRead < OriginalBlockSize)
                        {
                            OriginalLastBlockLength = bytesRead;
                        }

                        var blockBuffer = new byte[bytesRead];
                        Array.Copy(buffer, blockBuffer, blockBuffer.Length);
                        InputBlocks.Enqueue(new ByteBlock(blockBuffer, blocksRead));
                        blocksRead++;
                    }

                    InputBlocks.CompleteFilling();
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                throw;
            }
            
        }

        /// <summary>
        /// Writes to a destination file
        /// </summary>
        /// <param name="destinationFileInfo">Destination file</param>
        protected override void WriteToDestination(FileInfo destinationFileInfo)
        {
            try
            {
                using (var fileStream = new FileStream(destinationFileInfo.FullName, FileMode.Create))
                {
                    fileStream.Position = ZipUtils.FileHeaderSize;
                    var nextBlockNumber = 0;

                    while (true)
                    {
                        var result = OutputBlocks.TryTakeAndRemove(nextBlockNumber, out var block, true);

                        if (!result)
                        {
                            break;
                        }

                        nextBlockNumber++;
                        fileStream.Write(block, 0, block.Length);
                    }

                    WriteFileHeader(fileStream);
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                throw;
            }
            
        }

        /// <summary>
        /// Write a header information of a compressed file to the start of a FileStream
        /// </summary>
        /// <param name="fileStream">File stream</param>
        private void WriteFileHeader(FileStream fileStream)
        {
            fileStream.Seek(0, SeekOrigin.Begin);

            var fileHeader = new byte[ZipUtils.FileHeaderSize];
            BitConverter.GetBytes(OriginalFileSize).CopyTo(fileHeader, 0);
            BitConverter.GetBytes(OriginalBlockSize).CopyTo(fileHeader, sizeof(long));
            BitConverter.GetBytes(OriginalLastBlockLength).CopyTo(fileHeader, sizeof(int) + sizeof(long));
            fileStream.Write(fileHeader, 0, fileHeader.Length);

            fileStream.Seek(0, SeekOrigin.End);
        }
    }
}
