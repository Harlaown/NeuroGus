﻿using System.Collections.Generic;
using System.Text.RegularExpressions;
using NeuroGus.Core.Parser;

namespace NeuroGus.Core.Model
{
    public class FilteredUnigram : INGram
    {
        public HashSet<string> GetNGram(string text)
        {
            // get all significant words
            var words = Regex.Split(Clean(text), $@"[ \n\t\r$+<>№=]");

            // remove endings of words
            for (int i = 0; i < words.Length; i++)
            {
                words[i] = PorterStemmer.TransformingWord(words[i]);
            }

            var uniqueValues = new HashSet<string>(words);
            uniqueValues.RemoveWhere((s)=>s.Equals(""));

            return uniqueValues;
        }
        private static string Clean(string text)
        {
            
            // remove all digits and punctuation marks
            if (text == null) return "";
            var fixtext = Regex.Replace(text.ToLower(), @"[\\pP\\d]", " ");
            return fixtext;
        }
    }
}