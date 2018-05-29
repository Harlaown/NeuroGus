using System;
using System.Collections.Generic;

namespace NeuroGus.Core.Model
{
    public class VocabularyBuilder
    {
        private readonly INGram _inGram;

        public VocabularyBuilder(INGram inGram)
        {
            _inGram = inGram ?? throw new ArgumentNullException();
        }

        public List<VocabularyWord> GetVocabulary(List<ClassifiableText> classifiableTexts)
        {
            //if (classifiableTexts == null ||
            //    classifiableTexts.Count == 0)
            //{
            //    throw new ArgumentNullException();
            //}

            var uniqueValues = new Dictionary<string, int>();
            var vocabulary = new List<VocabularyWord>();

            // count frequency of use each word (converted to n-gram) from all Classifiable Texts
            //

            foreach (var classifiableText in classifiableTexts)
            {
                foreach (var word in _inGram.GetNGram(classifiableText.Text))
                {
                    if (uniqueValues.ContainsKey(word))
                    {
                        // increase counter
                        var i = uniqueValues[word];
                        uniqueValues[word] = i + 1;
                    }
                    else
                    {
                        // add new word
                        uniqueValues.Add(word, 1);
                    }
                }
            }


            // convert uniqueValues to Vocabulary, excluding infrequent
            //

            foreach (var entry in uniqueValues)
            {
                if (entry.Value > 1)
                {
                    vocabulary.Add(new VocabularyWord(entry.Key));
                }
            }

            return vocabulary;
        }
    }
}