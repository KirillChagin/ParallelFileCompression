using System;
using System.Diagnostics;
using System.IO;
using System.IO.Compression;

namespace CompressionLib.MultithreadedZip
{
    internal class Compressor : ZipBase
    {
        public Compressor(FileInfo sourceFile, FileInfo destinationFile) : base(sourceFile, destinationFile)
        {
            OriginalBlockSize = ZipUtils.BufferSize;
            OriginalFileSize = sourceFile.Length;
            OriginalLastBlockLength = OriginalBlockSize;
        }

        protected override void ProcessBlock(ByteBlock block)
        {
            if (block == null)
            {
                return; //TODO: throw?
            }

            using (var memoryStream = new MemoryStream())
            {
                using (var gZipStream = new GZipStream(memoryStream, CompressionMode.Compress))
                {
                    gZipStream.Write(block.Buffer, 0, block.Buffer.Length);
                }

                var compressedBlock = memoryStream.ToArray().AddByteBlockLengthHeader();
                OutputBlocks.Add(block.BlockNumber, compressedBlock);
                Trace.WriteLine("Block compressed " + block.BlockNumber + " " + block.Buffer.Length);
            }
        }

        protected override void ReadSource(FileInfo fileInfo)
        {
            var buffer = new byte[OriginalBlockSize];

            using (var fileStream = new FileStream(fileInfo.FullName, FileMode.Open, FileAccess.Read, FileShare.Read))
            {
                int bytesRead;
                var blocksRead = 0;
                while ((bytesRead = fileStream.Read(buffer, 0, buffer.Length)) > 0)
                {
                    if (bytesRead < OriginalBlockSize)
                    {
                        var lessBuffer = new byte[bytesRead];
                        Array.Copy(buffer, lessBuffer, lessBuffer.Length);
                        OriginalLastBlockLength = bytesRead;
                        InputBlocks.Enqueue(new ByteBlock(lessBuffer, blocksRead, true));
                    }
                    else
                    {
                        InputBlocks.Enqueue(new ByteBlock(buffer, blocksRead));
                    }

                    Trace.WriteLine("Blocks read " + blocksRead);
                    blocksRead++;
                }

                InputBlocks.CompleteFilling();
            }
        }

        protected override void WriteToDestination(FileInfo destinationFileInfo)
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
                    Trace.WriteLine("Blocks written " + (nextBlockNumber - 1));
                }

                WriteFileHeader(fileStream);
            }
        }

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
