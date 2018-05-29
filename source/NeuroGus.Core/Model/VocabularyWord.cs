using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Data.Entity;
namespace NeuroGus.Core.Model
{
    public partial class VocabularyWord
    {
        [Key]
        public int Id { get; set; }
        public string Value { get; set; }
        public VocabularyWord()
        {
        }

        public VocabularyWord(int id, string value)
        {
            Id = id;
            Value = value;
        }

        public VocabularyWord(string value)
        {
            Id = 0;
            Value = value;
        }



        public override bool Equals(object o)
        {
            return ((o is VocabularyWord word) && (this.Value.Equals(word.Value)));
        }

        public override int GetHashCode()
        {
            return Value.GetHashCode();
        }
    }
}