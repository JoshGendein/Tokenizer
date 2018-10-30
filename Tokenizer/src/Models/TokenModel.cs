using System.Collections.Generic;

namespace Tokenizer.src.Models
{
    public class TokenModel
    {
        public string Word { get; set; }
        public string DocumentId { get; set; }
        public float TF { get; set; }
        public float TFiDF { get; set; }
        public List<int> Positions { get; set; }
    }
}
