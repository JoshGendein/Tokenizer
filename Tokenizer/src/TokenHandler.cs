using System;
using System.Collections.Generic;
using System.Threading;
using Tokenizer.src.Models;
using System.Text.RegularExpressions;

namespace Tokenizer.src
{
    public class TokenHandler
    {
        private static readonly Regex regex = new Regex("[A-Za-z]+");
        private List<String> Files { get; set; }
        private Thread[] threads;
        private static SemaphoreSlim DatabaseLock = new SemaphoreSlim(1, 1);
        public TokenHandler(List<String> files, int threadCount)
        {
            this.Files = files;
            this.threads = new Thread[threadCount];

            CosmosDB<Page>.Initialize();

        }

        public void Run()
        {
            for (int i = 0; i < threads.Length; i++)
            {
                var Paths = DivideFiles(i);
                Thread currentThread = new Thread(() => ParseFiles(Paths));
                threads[i] = currentThread;
                currentThread.Start();
            }

            foreach (var thread in threads)
            {
                thread.Join();
            }
        }

        private void ParseFiles(List<string> paths)
        { 
            foreach(var path in paths)
            {
                var lines = System.IO.File.ReadAllLines(path);
                var document = Tokenize(lines);

                AddToDBAsync(document);
            }
        }

        private async void AddToDBAsync(Page document)
        {
            DatabaseLock.Wait();
            await CosmosDB<Page>.CreateItemAsync(document);
            DatabaseLock.Release();
        }

        private Page Tokenize(string[] lines)
        {
            var document = new Models.Page();
            var tokens = new Dictionary<string, int>();

            foreach(var line in lines)
            {
                foreach (Match match in regex.Matches(line))
                {
                    if (!tokens.TryGetValue(match.Value, out int currentCount))
                        tokens.Add(match.Value, 1);
                    else
                        tokens[match.Value] = currentCount + 1;
                    document.TotalWordCount++;
                }
            }
            document.Tokens = tokens;
            return document;
        }

        //Takes the full list of all files and divides them into smaller lists so each thread can access an individual set.
        private List<String> DivideFiles(int index)
        {
            var finalSet = new List<String>();

            for (int i = 0; i < Files.Count; i++)
            {
                if ((i % threads.Length) == index)
                {
                    finalSet.Add(Files[i]);
                }
            }

            return finalSet;
        }


    }
}
