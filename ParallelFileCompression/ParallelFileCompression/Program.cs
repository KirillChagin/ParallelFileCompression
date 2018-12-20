using System;
using CompressionLib;
using CompressionLib.MultithreadedZip;

namespace ParallelFileCompression
{
    class Program
    {
        public const int SuccessCode = 0;
        public const int ErrorCode = 1;

        private static IZip _zipProcessor;

        static void Main(string[] args)
        {
            AppDomain.CurrentDomain.UnhandledException += (o, e) =>
            {
                ExitWithError("Oops! It was unexpected.");
            };

            Console.CancelKeyPress += CancelKeyPress;

            var validationResult = CommandLineArgumentsValidator.ValidateArguments(args);

            if (!validationResult.Success)
            {
                ExitWithError(validationResult.ErrorMessage);
            }

            try
            {
                _zipProcessor = new MultithreadedZip();
                string successMessage = "";
                switch (validationResult.Command)
                {
                    case CommandLineArgumentsValidator.Command.Compress:
                    {
                        Console.WriteLine("Compression started");
                        _zipProcessor.Compress(validationResult.SourceFileInfo, validationResult.DestinationFileInfo);
                        successMessage = "File was successfully compressed";
                        break;
                    }
                    case CommandLineArgumentsValidator.Command.Decompress:
                    {
                        Console.WriteLine("Decompression started");
                        _zipProcessor.Decompress(validationResult.SourceFileInfo, validationResult.DestinationFileInfo);
                        successMessage = "File was successfully decompressed";
                        break;
                    }
                }

                if (_zipProcessor.Success)
                {
                    ExitSuccess(successMessage);
                }
                else
                {
                    ExitWithError(_zipProcessor.ErrorMessage);
                }
            }
            catch (Exception e)
            {
                ExitWithError($"Error occured: {e.Message}");
            }
        }

        private static void CancelKeyPress(object sender, ConsoleCancelEventArgs e)
        {
            _zipProcessor.Abort();
            e.Cancel = true;
            ExitWithError("Operation cancelled");
        }

        private static void ExitSuccess(string message)
        {
            Console.WriteLine(message);
            Console.ReadKey();
            Environment.Exit(SuccessCode);
        }

        private static void ExitWithError(string error)
        {
            Console.WriteLine(error);
            Console.ReadKey();
            Environment.Exit(ErrorCode);
        }
    }
}
