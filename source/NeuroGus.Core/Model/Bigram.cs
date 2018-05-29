using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NeuroGus.Core.Model
{
    public class Bigram : INGram
    {
        private readonly INGram _nGram;

        public Bigram(INGram nGram)
        {
            _nGram = nGram ?? throw new ArgumentNullException();
        }

        public HashSet<string> GetNGram(string text)
        {
            var unigram = new List<string>(_nGram.GetNGram(text));

            // concatenate words to bigrams
            // example: "How are you doing?" => {"how are", "are you", "you doing"}

            var uniqueValues = new HashSet<string>();

            for (var i = 0; i < unigram.Count - 1; i++) uniqueValues.Add(unigram[i] + " " + unigram[i + 1]);

            return uniqueValues;
        }
    }
}
