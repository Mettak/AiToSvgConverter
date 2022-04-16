using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace AiToSvgConverter
{
    public class Program
    {
        static void Main(string[] args)
        {
            List<string> argumentsAsList = args.ToList();

            try
            {
                if (args.ElementAtOrDefault(0) == "--help")
                {
                    Console.WriteLine("List of parameters:\n");
                    Console.WriteLine("source-file\t\tSpecifies source .ai file for converting");
                    Console.WriteLine("target-file\t\tSpecifies where converted .svg file will be saved");
                    Console.WriteLine("fill-color\t\tSpecifies fill color for .svg file - default value is #000000 (black)");
                    Console.WriteLine("overwrite\t\tDeterminates if target file can be overwritten, if not, application throws an exception");
                    Console.ReadLine();
                    return;
                }

                string sourceFilePath = GetParameterValue<string>(argumentsAsList, "source-file");
                string targetFilePath = GetParameterValue<string>(argumentsAsList, "target-file");
                string fillHexColor = GetParameterValue<string>(argumentsAsList, "fill-color") ?? "#000000";
                bool overwrite = GetParameterValue<bool>(argumentsAsList, "overwrite");

                if (sourceFilePath == null)
                {
                    throw new ArgumentNullException("source-file");
                }

                if (targetFilePath == null)
                {
                    throw new ArgumentNullException("target-file");
                }

                string[] dataLines = File.ReadAllLines(sourceFilePath);
                bool targetFileExists = File.Exists(targetFilePath);
                AiFile aiFile = AiFileDeserializer.Deserialize(dataLines);

                if ((targetFileExists && overwrite) || !targetFileExists)
                {
                    File.WriteAllText(targetFilePath, SvgConverter.FromAiFileToSvgString(aiFile, fillHexColor));
                }

                else
                {
                    throw new Exception($@"Target file already exists! Use parameter ""--overwrite true"" for overwriting existing files.");
                }
            }

            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                Console.WriteLine("Use parameter --help for list of available parameters");
                Console.ReadLine();
            }
        }

        static T GetParameterValue<T>(List<string> args, string parameter)
        {
            int sourceFileIndex = args.FindIndex(x => x == $"--{parameter}");
            if (sourceFileIndex == -1)
            {
                return default;
            }

            string value = args.ElementAtOrDefault(sourceFileIndex + 1);
            if (value == null)
            {
                return default;
            }

            return (T)Convert.ChangeType(value, typeof(T));
        }
    }
}
