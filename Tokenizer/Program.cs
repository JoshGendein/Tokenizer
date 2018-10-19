using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using Tokenizer.src;

namespace Tokenizer
{
    public class Program
    {
        private static List<String> documents;
        private static int maxThreads = Environment.ProcessorCount;
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

            var threads = new Thread[maxThreads];

            for(int i=0; i < maxThreads; i++)
            {
                var thisHandlersDocumentSet = DivideDocuments(i);
                var handler = new TokenHandler(thisHandlersDocumentSet);
                var handlerThread = new Thread(new ThreadStart(handler.tokenize));
                threads[i] = handlerThread;
                handlerThread.Start();
            }

            foreach(var thread in threads)
            {
                thread.Join();
            }

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

        //Takes the list of all documents and divides them into smaller lists so each thread can access them.
        public static List<String> DivideDocuments(int index)
        {
            var finalSet = new List<String>();

            for(int i=0; i < documents.Count; i++)
            {
                if((i % maxThreads) == index)
                {
                    finalSet.Add(documents[i]);
                }
            }

            return finalSet;
        }
    }
}
