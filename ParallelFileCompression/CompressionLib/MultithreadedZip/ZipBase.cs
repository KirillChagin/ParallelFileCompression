using System;
using System.Diagnostics;
using System.IO;
using System.Threading;
using ZipThreading.CollectionProcessorThreadPool;
using ZipThreading.Collections;

namespace CompressionLib.MultithreadedZip
{
    internal abstract class ZipBase
    {
        private readonly FileInfo _sourceFile;
        private readonly FileInfo _destinationFile;

        protected readonly SimpleConcurrentQueue<ByteBlock> InputBlocks;
        protected readonly SimpleConcurrentDictionary<int, byte[]> OutputBlocks;

        protected long OriginalFileSize;
        protected int OriginalBlockSize;
        protected int OriginalLastBlockLength;

        private CollectionProcessorThreadPool<ByteBlock> _threadPool;
        private Thread _writeThread;


        protected ZipBase(FileInfo sourceFile, FileInfo destinationFile)
        {
            _sourceFile = sourceFile;
            _destinationFile = destinationFile;

            InputBlocks = new SimpleConcurrentQueue<ByteBlock>();
            OutputBlocks = new SimpleConcurrentDictionary<int, byte[]>();
        }

        public void Start()
        {
            StartReadSource();

            StartProcession();

            StartWriteToDestination();

            WaitProcession();

            WaitWriteToDestination();
        }

        #region Reading

        private void StartReadSource()
        {
            var readThread = new Thread(ReadSource);
            readThread.Start();
        }

        private void ReadSource()
        {
            ReadSource(_sourceFile);
        }

        protected abstract void ReadSource(FileInfo fileInfo);

        #endregion

        #region Processing

        private void StartProcession()
        {
            _threadPool = new CollectionProcessorThreadPool<ByteBlock>(ProcessBlock, InputBlocks);
            _threadPool.StartPool();
        }

        protected abstract void ProcessBlock(ByteBlock block);

        private void WaitProcession()
        {
            _threadPool.WaitAll();
            OutputBlocks.CompleteFilling();
        }

        #endregion

        #region Writing

        private void StartWriteToDestination()
        {
            _writeThread = new Thread(WriteToDestination);
            _writeThread.Start();
        }

        private void WriteToDestination()
        {
            WriteToDestination(_destinationFile);
        }

        protected abstract void WriteToDestination(FileInfo destinationFileInfo);     

        private void WaitWriteToDestination()
        {
            _writeThread.Join();
        }

        #endregion
    }
}
