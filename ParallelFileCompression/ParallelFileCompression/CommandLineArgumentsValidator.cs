using System;
using System.IO;

namespace ParallelFileCompression
{
    public class CommandLineArgumentsValidator
    {
        public enum Command
        {
            Compress,
            Decompress
        }

        public class ValidationResult
        {
            public bool Success { get; set; }
            public string ErrorMessage { get; set; }
            public Command Command { get; set; }
            public FileInfo SourceFileInfo { get; set; }
            public FileInfo DestinationFileInfo { get; set; }
        }

        public const string CompressCommand = "compress";
        public const string DecompressCommand = "decompress";

        public const string IncorrectArgumentsMessage = "Incorrect arguments passed. ";
        public const string IncorrectCommandMessage = "Incorrect command";
        public const string UseThisPatternMessage =
            "Please enter arguments in the following format:\n compress(decompress) [Source file full path] [Destination file full path]";

        public const string SourceFileNotExistsMessage = "Source file not exists.";
        public const string EqualSourceAndDestinationMessage = "Source and destination can't have the same path";

        public static ValidationResult ValidateArguments(string[] args)
        {
            var validationResult = new ValidationResult() { ErrorMessage = null, Success = false };

            if (args.Length != 3)
            {
                validationResult.ErrorMessage = IncorrectArgumentsMessage + UseThisPatternMessage;
                return validationResult;
            }

            if (args[0] == CompressCommand)
            {
                validationResult.Command = Command.Compress;
            }
            else if (args[0] == DecompressCommand)
            {
                validationResult.Command = Command.Decompress;
            }
            else
            {
                validationResult.ErrorMessage = IncorrectCommandMessage + UseThisPatternMessage;
                return validationResult;
            }

            if (!File.Exists(args[1]))
            {
                validationResult.ErrorMessage = SourceFileNotExistsMessage;
                return validationResult;
            }

            if (args[1] == args[2])
            {
                validationResult.ErrorMessage = EqualSourceAndDestinationMessage;
                return validationResult;
            }

            if (File.Exists(args[2]))
            {
                var question =
                    "Destination file is already exists. If you're agree it would be rewritten. But if something goes wrong, file would be deleted because I was too lazy to implement correct recovery mechanism. \n Do you want to continue? [Y]/[N] ";
                var result = AskForContinue(question);
                if (!result)
                {
                    Environment.Exit(0);
                }
            }

            validationResult.SourceFileInfo = new FileInfo(args[1]);
            validationResult.DestinationFileInfo = new FileInfo(args[2]);
            validationResult.Success = true;
            return validationResult;
        }

        private static bool AskForContinue(string question)
        {
            ConsoleKey response;
            do
            {
                Console.Write(question);
                response = Console.ReadKey(false).Key;
                if (response != ConsoleKey.Enter)
                    Console.WriteLine();

            } while (response != ConsoleKey.Y && response != ConsoleKey.N);

            return response == ConsoleKey.Y;
        }
    }
}
