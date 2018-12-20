using System;
using System.IO;
using System.Threading;
using ZipThreading.CollectionProcessorThreadPool;
using ZipThreading.Collections;

namespace CompressionLib.MultithreadedZip
{
    /// <summary>
    /// Base class for file compression an decompression
    /// </summary>
    internal abstract class ZipBase : IDisposable
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

        protected bool IsAborted;

        public bool Success { get; protected set; }
        public string ErrorMessage { get; protected set; }
        
        protected ZipBase(FileInfo sourceFile, FileInfo destinationFile)
        {
            _sourceFile = sourceFile;
            _destinationFile = destinationFile;

            InputBlocks = new SimpleConcurrentQueue<ByteBlock>();
            OutputBlocks = new SimpleConcurrentDictionary<int, byte[]>();
        }

        /// <summary>
        /// Start execution process
        /// </summary>
        public void Start()
        {
            StartReadSource();

            StartProcession();

            StartWriteToDestination();

            WaitProcession();

            WaitWriteToDestination();

            if (IsAborted && File.Exists(_destinationFile.FullName))
            {
                File.Delete(_destinationFile.FullName);
                return;
            }

            Success = true;
        }

        #region Reading

        private void StartReadSource()
        {
            var readThread = new Thread(ReadSource);
            readThread.Start();
        }

        private void ReadSource()
        {
            try
            {
                ReadSource(_sourceFile);
            }
            catch (Exception e)
            {
                ErrorMessage = $"Error occured while reading the file {_sourceFile.FullName}. {e.Message}";
                Abort();
            }
        }

        /// <summary>
        /// Reads of a source file
        /// </summary>
        /// <param name="fileInfo">Source file</param>
        protected abstract void ReadSource(FileInfo fileInfo);

        #endregion

        #region Processing

        private void StartProcession()
        {
            _threadPool = new CollectionProcessorThreadPool<ByteBlock>(block =>
            {
                try
                {
                    ProcessBlock(block);
                }
                catch (Exception e)
                {
                    ErrorMessage = $"Error occured while compressing(decompressing) the file. {e.Message}";
                    Abort();
                }
            }, InputBlocks);

            _threadPool.StartPool();
        }

        /// <summary>
        /// Procession of a block method that is used as callback for <see cref="CollectionProcessorThreadPool{T}"/>
        /// </summary>
        /// <param name="block">Processing block</param>
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
            _writeThread = new Thread(WriteToDestination)
            {
                Priority = ThreadPriority.AboveNormal //Important for slow disks. Otherwise output collection can grow very fast
            };
            _writeThread.Start();
        }

        private void WriteToDestination()
        {
            try
            {
                WriteToDestination(_destinationFile);
            }
            catch (Exception e)
            {
                ErrorMessage = $"Error occured while writing to the file: {_destinationFile.FullName} {e.Message}";
                Abort();
            }
        }

        /// <summary>
        /// Writes to a destination file
        /// </summary>
        /// <param name="destinationFileInfo">Destination file</param>
        protected abstract void WriteToDestination(FileInfo destinationFileInfo);     

        private void WaitWriteToDestination()
        {
            _writeThread.Join();
        }

        #endregion

        public void Abort()
        {
            IsAborted = true;
            InputBlocks.Flush();
            OutputBlocks.Flush();
        }

        public void Dispose()
        {
            //Just to collect memory after work
            GC.Collect();
            GC.SuppressFinalize(this);
        }
    }
}
