namespace NeuroGus.Core.Model
{
    public static class ExtendedClass
    {
        public static INGram GetNGram(this INGram ingram)
        {
            switch (ingram.GetType().ToString().ToLower())
            {
                case "unigram":
                    return new Unigram();
                case "filteredunigram":
                    return new FilteredUnigram();
                case "bigram":
                    return new Bigram(new Unigram());
                case "filteredbigram":
                    return new Bigram(new FilteredUnigram());
                default:
                    return null;
            }
        }
    }
}