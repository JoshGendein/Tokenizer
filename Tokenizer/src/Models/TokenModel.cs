using System.ComponentModel.DataAnnotations;

namespace Tokenizer.src.Models
{
    public class TokenModel
    {
        [Key]
        public string Word { get; set; }
        [Key]
        public string DocumentId { get; set; }
        public float TF { get; set; }
        public float TFiDF { get; set; }
    }
}
