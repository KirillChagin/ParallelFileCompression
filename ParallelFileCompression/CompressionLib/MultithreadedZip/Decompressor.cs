using System;
using System.IO;
using System.Threading;

namespace CompressionLib.MultithreadedZip
{
    internal class Decompressor : ZipBase
    {
        public Decompressor(FileInfo sourceFile, FileInfo destinationFile) : base(sourceFile, destinationFile)
        {
        }

        protected override void StartProcession()
        {
            throw new NotImplementedException();
        }

        protected override void ReadSource()
        {
            var readThread = new Thread(base.ReadSource);
            readThread.Start();
        }

        protected override void WriteToDestination()
        {
            var writeThread = new Thread(base.WriteToDestination);
            writeThread.Start();
        }
    }
}
