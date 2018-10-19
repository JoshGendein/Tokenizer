using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;

namespace Tokenizer.src
{
    public class TokenHandler
    {
        private List<String> Documents { get; set; }
        private Thread[] threads;
        public TokenHandler(List<String> documents, int threadCount)
        {
            this.Documents = documents;
            this.threads = new Thread[threadCount];

            //Establish DB connection.

        }

        public void Run()
        {
            for (int i = 0; i < threads.Length; i++)
            {
                var thisHandlersDocumentSet = DivideDocuments(i);
                var handlerThread = new Thread(new ThreadStart(this.Tokenize));
                threads[i] = handlerThread;
                handlerThread.Start();
            }

            foreach (var thread in threads)
            {
                thread.Join();
            }
        }

        private void Tokenize()
        {
            
        }

        //Takes the list of all documents and divides them into smaller lists so each thread can access their individual set.
        private List<String> DivideDocuments(int index)
        {
            var finalSet = new List<String>();

            for (int i = 0; i < Documents.Count; i++)
            {
                if ((i % threads.Length) == index)
                {
                    finalSet.Add(Documents[i]);
                }
            }

            return finalSet;
        }


    }
}
