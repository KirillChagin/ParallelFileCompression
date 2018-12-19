using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using CompressionLib.MultithreadedZip;

namespace TestProject
{
    public class Program
    {
        private static readonly string ssdFileNameSmall = @"C:\Projects\TestFiles\TestSmall.pdf";
        private static readonly string hddFileNameSmall = @"D:\Projects\TestFiles\TestSmall.pdf";

        private static readonly string ssdFileNameLarge = @"C:\Projects\TestFiles\TestLarge.avi";
        private static readonly string hddFileNameLarge = @"D:\Projects\TestFiles\TestLarge.avi";

        private static readonly string ssdFileNameMedium = @"C:\Projects\TestFiles\TestMedium.avi";
        private static readonly string hddFileNameMedium = @"D:\Projects\TestFiles\TestMedium.avi";

        public static void Main()
        {
            //SmallTest();

            MediumTest();

            //LargeTest();

            Console.ReadLine();
        }

        private static void SmallTest()
        {
            Console.WriteLine("Small file:");
            //SSD compression
            TestCompression("SSD", ssdFileNameSmall);

            //SSD decompression
            TestDecompression("SSD", ssdFileNameSmall);

            //HDD compression
            //TestCompression("HDD", hddFileNameSmall);

            //HDD decompression
            //TestDecompression("HDD", hddFileNameSmall);

            Console.WriteLine();
        }

        private static void MediumTest()
        {
            Console.WriteLine("Medium file:");

            //SSD compression
            TestCompression("SSD", ssdFileNameMedium);

            //SSD decompression
            TestDecompression("SSD", ssdFileNameMedium);

            //HDD compression
            //TestCompression("HDD", hddFileNameMedium);

            //HDD decompression
            //TestDecompression("HDD", hddFileNameMedium);

            Console.WriteLine();
        }

        private static void LargeTest()
        {
            Console.WriteLine("Large file:");

            //SSD compression
            TestCompression("SSD", ssdFileNameLarge);

            //SSD decompression
            //TestDecompression("SSD", ssdFileNameLarge);

            //HDD compression
            //TestCompression("HDD", hddFileNameLarge);

            //HDD decompression
            //TestDecompression("HDD", hddFileNameLarge);

            Console.WriteLine();
        }


        private static void TestCompression(string diskType, string fileName)
        {
            var fileInfo = new FileInfo(fileName);
            var destFileInfo = new FileInfo(fileName + ".gz");

            Console.WriteLine("{0} compression test started", diskType);
            var watch = System.Diagnostics.Stopwatch.StartNew();

            var zip = new MultithreadedZip();
            zip.Compress(fileInfo, destFileInfo);

            watch.Stop();
            Console.WriteLine("{0} compression test finished. Elapsed time {1}", diskType, watch.ElapsedMilliseconds);
        }

        private static void TestDecompression(string diskType, string fileName)
        {
            var fileInfo = new FileInfo(fileName + ".gz");
            var currentFileName = fileInfo.FullName;
            var newFileName = currentFileName.Remove(fileInfo.FullName.Length - fileInfo.Extension.Length).Replace(".", "(1).");

            var destFileInfo = new FileInfo(newFileName);

            Console.WriteLine("{0} decompression test started", diskType);
            var watch = System.Diagnostics.Stopwatch.StartNew();

            var zip = new MultithreadedZip();
            zip.Decompress(fileInfo, destFileInfo);

            watch.Stop();
            Console.WriteLine("{0} decompression test finished. Elapsed time {1}", diskType, watch.ElapsedMilliseconds);
        }
    }
}
