using System;
using System.Collections.Generic;
using System.IO;

namespace Tokenizer.src
{
    public class TokenHandler
    {
        private int index { get; set; }
        private List<String> files { get; set; }
        public TokenHandler(int index, List<String> files)
        {
            this.index = index;
            this.files = files;
        }

        public void tokenize()
        {
            foreach(String s in files)
            {
                Console.WriteLine("Thread {0}", index);
            }
        }

        
    }
}
