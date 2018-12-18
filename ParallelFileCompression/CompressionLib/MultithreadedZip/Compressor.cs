using System;
using System.Diagnostics;
using System.IO;
using System.IO.Compression;
using System.Threading;
using ZipThreading.CollectionProcessorThreadPool;

namespace CompressionLib.MultithreadedZip
{
    internal class Compressor : ZipBase
    {
        private CollectionProcessorThreadPool<ByteBlock> _threadPool;

        private Thread _writeThread;

        public Compressor(FileInfo sourceFile, FileInfo destinationFile) : base(sourceFile, destinationFile)
        {
        }

        protected override void StartProcession()
        {
            _threadPool = new CollectionProcessorThreadPool<ByteBlock>(CompressBlock, InputBlocks);
            _threadPool.StartPool();
        }

        protected override void WaitProcession()
        {
            _threadPool.WaitAll();
            OutputBlocks.CompleteFilling();
        }

        protected override void WaitWriteToDestination()
        {
            _writeThread.Join();
        }

        protected override void ReadSource()
        {
            var readThread = new Thread(base.ReadSource);
            readThread.Start();
        }

        protected override void WriteToDestination()
        {
            _writeThread = new Thread(base.WriteToDestination);
            _writeThread.Start();
        }

        private void CompressBlock(ByteBlock block)
        {
            if (block == null)
            {
                return; //TODO: throw?
            }

            using (MemoryStream memoryStream = new MemoryStream())
            {
                using (GZipStream gZipStream = new GZipStream(memoryStream, CompressionMode.Compress))
                {
                    gZipStream.Write(block.Buffer, 0, block.Buffer.Length);
                }

                byte[] compressedData = memoryStream.ToArray();
                OutputBlocks.Add(block.BlockNumber, compressedData);
                Trace.WriteLine("Block compressed " + block.BlockNumber);
            }
        }
    }
}
