using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;

namespace NeuroGus.Core.Model
{
    public sealed partial class ClassifiableText
    {
        
        public ClassifiableText()
        {
            Characteristics = new HashSet<KeyValuePair>();
        }
        [Key]
        public int Id { get; set; }
        public string Text { get; set; }
        public ICollection<KeyValuePair> Characteristics { get; set; }


        public ClassifiableText(string text, ICollection<KeyValuePair> characteristics)
        {
            Text = text;
            Characteristics = characteristics;
        }

        public ClassifiableText(string text) : this(text, null)
        {
        }

        public CharacteristicValue GetCharacteristicValue(string characteristicName)
        {
            var value = (from v in Characteristics
                where v.Key.Name == characteristicName
                select v.Value).FirstOrDefault();
            return value;
        }
    }
}