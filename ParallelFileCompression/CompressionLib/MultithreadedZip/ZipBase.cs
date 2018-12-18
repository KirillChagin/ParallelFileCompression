using System;
using System.IO;
using ZipThreading.Collections;

namespace CompressionLib.MultithreadedZip
{
    internal abstract class ZipBase
    {
        private readonly FileInfo _sourceFile;
        private readonly FileInfo _destinationFile;

        protected readonly SimpleConcurrentQueue<byte[]> InputBlocks;
        protected readonly SimpleConcurrentDictionary<int, byte[]> OutputBlocks;

        protected ZipBase(FileInfo sourceFile, FileInfo destinationFile)
        {
            _sourceFile = sourceFile;
            _destinationFile = destinationFile;

            InputBlocks = new SimpleConcurrentQueue<byte[]>();
            OutputBlocks = new SimpleConcurrentDictionary<int, byte[]>();
        }

        public void Execute()
        {
            ReadSource();

            StartProcession();

            WriteToDestination();
        }

        protected virtual void ReadSource()
        {
            var buffer = new byte[ZipUtils.BufferSize];

            using (var fileStream = new FileStream(_sourceFile.FullName, FileMode.Open))
            {
                int bytesRead;
                while ((bytesRead = fileStream.Read(buffer, 0, buffer.Length)) > 0)
                {
                    if (bytesRead <= ZipUtils.BufferSize)
                    {
                        var lessBuffer = new byte[bytesRead];
                        Array.Copy(buffer, lessBuffer, lessBuffer.Length);
                        InputBlocks.Enqueue(lessBuffer);
                    }
                    else
                    {
                        InputBlocks.Enqueue(buffer);
                    }
                }
            }
        }

        protected virtual void WriteToDestination()
        {
            //TODO: convert fileName (add extension if it is incorrect)
            using (var fileStream = new FileStream(_destinationFile.FullName, FileMode.Append))
            {
                var nextBlockNumber = 0;
                while (true)
                {
                    var result = OutputBlocks.TryTakeAndRemove(nextBlockNumber, out var block, true);

                    if (!result)
                    {
                        return;
                    }

                    nextBlockNumber++;
                    fileStream.Write(block, 0, block.Length);
                }
            }
        }

        protected abstract void StartProcession();
    }
}
