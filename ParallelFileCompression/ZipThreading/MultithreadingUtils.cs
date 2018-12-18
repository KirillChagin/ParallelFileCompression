using System;

namespace ZipThreading
{
    public static class MultithreadingUtils
    {
        public static int OptimalThreadsCount => Environment.ProcessorCount;
    }
}
