using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Tokenizer.src;


namespace Tokenizer
{
    public class Program
    {
        private static List<String> documents;
        private static readonly int maxThreads = Environment.ProcessorCount;
        static void Main(string[] args)
        {
            
            Console.WriteLine("Please enter a directory relative to this program:");
            var inputDirectory = Console.ReadLine();
            var directory = System.Environment.CurrentDirectory + inputDirectory;
            Console.WriteLine(directory);

            if(!CheckDirectory(directory))
            {
                return;
            }

            //Gather a list of file paths from directory.
            documents = Directory.EnumerateFiles(directory, "*.txt").ToList();

            var handler = new TokenHandler(documents, maxThreads);
            handler.Run();
        }
        public static bool CheckDirectory(String directory)
        {
            if (!Directory.Exists(directory))
            {
                Console.WriteLine("Error finding directory with path: {0}", directory);
                return false;
            }
            return true;
        }
    }
}
