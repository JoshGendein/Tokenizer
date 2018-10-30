using System;
using System.Collections.Generic;
using System.Threading;
using Tokenizer.src.Models;
using System.Text.RegularExpressions;
using System.IO;
using System.Data.SqlClient;

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

            foreach (var file in paths)
            {
                var tokens = Tokenize(file);

                AddToDB(tokens);
            }
        }

        private void AddToDB(List<TokenModel> tokens)
        {
            using (var connection = new SqlConnection())
            {
                var client = new DBClient(connection);
                
                client.BulkInsertTokens(tokens);
            }
        }

        private List<TokenModel> Tokenize(string path)
        {

            var wordList = new Dictionary<string, List<int>>();
            var wordCount = 0;

            var lines = File.ReadAllLines(path);
            foreach (var line in lines)
            {
                foreach (Match match in regex.Matches(line))
                {
                    wordCount++;
                    //Temporary fix because DB couldnt handle case sensitive PK.
                    var word = match.Value.ToLower();

                    if (!wordList.TryGetValue(word, out List<int> positionList))
                        wordList.Add(word, new List<int>());

                    wordList[word].Add(wordCount);

                }
            }
            var tokens = new List<TokenModel>();
            foreach (var word in wordList)
            {
                tokens.Add(new TokenModel
                {
                    Word = word.Key,
                    DocumentId = Path.GetFileName(path),
                    Positions = word.Value,
                    TF = (float) word.Value.Count / wordCount
                });
            }
            return tokens;
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
