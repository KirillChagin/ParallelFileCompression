using System;
using System.Diagnostics;
using System.IO;
using ZipThreading.Collections;

namespace CompressionLib.MultithreadedZip
{
    internal abstract class ZipBase
    {
        private readonly FileInfo _sourceFile;
        private readonly FileInfo _destinationFile;

        protected readonly SimpleConcurrentQueue<ByteBlock> InputBlocks;
        protected readonly SimpleConcurrentDictionary<int, byte[]> OutputBlocks;

        protected ZipBase(FileInfo sourceFile, FileInfo destinationFile)
        {
            _sourceFile = sourceFile;
            _destinationFile = destinationFile;

            InputBlocks = new SimpleConcurrentQueue<ByteBlock>();
            OutputBlocks = new SimpleConcurrentDictionary<int, byte[]>();
        }

        public void Start()
        {
            ReadSource();

            StartProcession();

            WriteToDestination();

            WaitProcession();

            WaitWriteToDestination();
        }

        protected virtual void ReadSource()
        {
            var buffer = new byte[ZipUtils.BufferSize];

            using (var fileStream = new FileStream(_sourceFile.FullName, FileMode.Open, FileAccess.Read, FileShare.Read, ZipUtils.BufferSize))
            {
                int bytesRead;
                var blocksRead = 0;
                while ((bytesRead = fileStream.Read(buffer, 0, buffer.Length)) > 0)
                {
                    if (bytesRead <= ZipUtils.BufferSize)
                    {
                        var lessBuffer = new byte[bytesRead];
                        Array.Copy(buffer, lessBuffer, lessBuffer.Length);
                        InputBlocks.Enqueue(new ByteBlock(lessBuffer, blocksRead));
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

        protected virtual void WriteToDestination()
        {
            //TODO: convert fileName (add extension if it is incorrect)
            using (var fileStream = new FileStream(_destinationFile.FullName, FileMode.Create))
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

        protected abstract void StartProcession();

        protected abstract void WaitProcession();

        protected abstract void WaitWriteToDestination();
    }
}
