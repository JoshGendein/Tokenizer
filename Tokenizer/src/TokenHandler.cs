using System;
using System.Collections.Generic;
using System.Threading;
using Tokenizer.src.Models;
using System.Text.RegularExpressions;
using System.IO;
using Microsoft.EntityFrameworkCore;

namespace Tokenizer.src
{
    public class TokenHandler
    {
        private static readonly Regex regex = new Regex("[A-Za-z]+");
        private List<String> Files { get; set; }
        private Thread[] threads;
        public TokenHandler(List<String> files, int threadCount)
        {
            this.Files = files;
            this.threads = new Thread[threadCount];
        }

        public void Run()
        {
            for (int i = 0; i < threads.Length; i++)
            {
                var Paths = DivideFiles(i);
                Thread currentThread = new Thread(() => ProcessFiles(Paths));
                threads[i] = currentThread;
                currentThread.Start();
            }

            foreach (var thread in threads)
            {
                thread.Join();
            }
        }

        private void ProcessFiles(List<string> paths)
        {
            var page = new Page();

            foreach (var path in paths)
            {
                var lines = File.ReadAllLines(path);
                var pageValues = Tokenize(lines);

                page.Tokens = pageValues.Item1;
                page.TotalWordCount = pageValues.Item2;
                page.Id = Path.GetFileName(path);

                AddToDB(page);
            }
        }

        private void AddToDB(Page page)
        {
            using (var db = new TokenizerDbContext())
            {
                var strategy = db.Database.CreateExecutionStrategy();

                strategy.Execute(() =>
                {
                    using (var context = new TokenizerDbContext())
                    {
                        using (var transaction = context.Database.BeginTransaction())
                        {
                            foreach (var word in page.Tokens)
                            {
                                var token = new TokenModel
                                {
                                    Word = word.Key,
                                    TF = (float)word.Value / page.TotalWordCount,
                                    DocumentId = page.Id
                                };
                                context.Tokens.Add(token);
                                context.SaveChanges();
                            }
                            transaction.Commit();
                        }

                    }
                });
            }
        }

        private ValueTuple<Dictionary<string, int>, int> Tokenize(string[] lines)
        {
            var tokens = new Dictionary<string, int>();
            var wordCount = 0;

            foreach(var line in lines)
            {
                foreach (Match match in regex.Matches(line))
                {
                    //Temporary fix because DB couldnt handle case sensitive PK.
                    var word = match.Value.ToLower();

                    if (!tokens.TryGetValue(word, out int currentCount))
                        tokens.Add(word, 1);
                    else
                        tokens[word] = currentCount + 1;
                    wordCount++;
                }
            }
            return (tokens, wordCount);
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
