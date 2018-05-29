using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace NeuroGus.Core.Model
{
    public class Unigram : INGram
    {
        public HashSet<string> GetNGram(string text)
        {
            if (text == null)
            {
                text = "";
            }

            // get all words and digits
            //var words = text.ToLower().Split("[ \\pP\n\t\r$+<>№=]");
            var words = Regex.Split(text.ToLower(), @"[ \\pP\n\t\r$+<>№=]");
            var uniqueValues = new HashSet<string>(words);
            uniqueValues.RemoveWhere((s) => s.Equals(""));
            return uniqueValues;
        }
    }
}
