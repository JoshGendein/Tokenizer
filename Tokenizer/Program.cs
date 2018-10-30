using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Diagnostics;
using System.IO;
using System.Linq;
using Tokenizer.src;

namespace Tokenizer
{
    public class Program
    {
        private static List<String> documents;
        private static readonly int threads = Environment.ProcessorCount;
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

            //Initialize DB
            using (var connection = new SqlConnection())
            {
                var client = new DBClient(connection);
                client.Initialize();
            }

            //Gather a list of file paths from directory.
            documents = Directory.EnumerateFiles(directory, "*.txt").ToList();

            Console.WriteLine("Using {0} threads to process {1} files", threads, documents.Count);
            var stopwatch = Stopwatch.StartNew();

            var handler = new TokenHandler(documents, threads);
            handler.Run();

            stopwatch.Stop();
            Console.WriteLine($"Elapsed time: {stopwatch.ElapsedMilliseconds}ms");
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
