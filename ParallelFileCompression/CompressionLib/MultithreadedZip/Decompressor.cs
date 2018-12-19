using System;
using System.Diagnostics;
using System.IO;
using System.IO.Compression;

namespace CompressionLib.MultithreadedZip
{
    internal class Decompressor : ZipBase
    {
        public Decompressor(FileInfo sourceFile, FileInfo destinationFile) : base(sourceFile, destinationFile)
        {
        }

        protected override void ProcessBlock(ByteBlock block)
        {
            if (block == null)
            {
                return; //TODO: throw?
            }

            using (var memoryStream = new MemoryStream(block.Buffer))
            {
                var decompressedBlock = block.IsLast ? new byte[OriginalLastBlockLength] : new byte[OriginalBlockSize];

                using (var gZipStream = new GZipStream(memoryStream, CompressionMode.Decompress))
                {
                    gZipStream.Read(decompressedBlock, 0, decompressedBlock.Length);
                }

                OutputBlocks.Add(block.BlockNumber, decompressedBlock);
                Trace.WriteLine("Block decompressed " + block.BlockNumber + " " + decompressedBlock.Length);
            }
        }

        protected override void ReadSource(FileInfo fileInfo)
        {
            using (var fileStream = new FileStream(fileInfo.FullName, FileMode.Open, FileAccess.Read, FileShare.Read))
            {
                var blocksRead = 0;
                long totalBytesRead = 0;
                var compressedFileSize = fileInfo.Length;

                ReadFileHeader(fileStream);
                totalBytesRead += ZipUtils.FileHeaderSize;

                while (true)
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
                        Trace.WriteLine("Blocks read " + blocksRead);
                        break;
                    }

                    InputBlocks.Enqueue(new ByteBlock(block, blocksRead));
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
