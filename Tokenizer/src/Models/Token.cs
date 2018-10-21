using System;
using System.Collections.Generic;
using System.Text;

namespace Tokenizer.src.Models
{
    class Token
    {
        private String Word { get; set; }

        private int TokenFrequency { get; set; }

        public Token(String Word)
        {
            this.Word = Word;
        }

        public void IncreaseTF()
        {
            this.TokenFrequency++;
        }

    }
}
