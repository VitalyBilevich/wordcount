namespace TextProcessor
{
    public class WordCounter: IWordCounter
    {
        public Dictionary<string, int> CountWords (string text)
        {   
            if(string.IsNullOrWhiteSpace(text))
            {
                return new Dictionary<string, int>();
            }

            var wordCounts = new Dictionary<string, int>();
            var words = text.Split(new[] { ' ', '\n', '\r', '\t', ',', '.', '!', '?' }, StringSplitOptions.RemoveEmptyEntries);

            foreach (var word in words)
            {
                var normalizedWord = word.ToLower();
                if (wordCounts.ContainsKey(normalizedWord))
                    wordCounts[normalizedWord]++;
                else
                    wordCounts[normalizedWord] = 1;
            }

            return wordCounts;
        }

        // TODO: Implement the CountWordsDFA method
        public Dictionary<string, int> CountWordsDFA(string text)
        {
            return new Dictionary<string, int>();
        }
    }
}
