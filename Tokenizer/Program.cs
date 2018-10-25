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
            var directory = Environment.CurrentDirectory + inputDirectory;
            Console.WriteLine(directory);

            if(!CheckDirectory(directory))
            {
                return;
            }

            var client = new DBClient();
            client.InitializeAsync("Tokenizer", "DF").Wait();
            
            //Gather a list of file paths from directory.
            documents = Directory.EnumerateFiles(directory, "*.txt").ToList();

            Console.WriteLine("Using {0} threads to process {1} files", maxThreads, documents.Count);
            Console.WriteLine("Starting time is {0}", DateTime.Now.ToString("h:mm:ss tt"));

            var handler = new TokenHandler(documents, maxThreads);
            handler.Run();

            Console.WriteLine("Ending time is {0}", DateTime.Now.ToString("h:mm:ss tt"));
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
