namespace TextProcessor
{
    public interface IWordCounter
    {
        public Dictionary<string, int> CountWords(string text);

        public Dictionary<string, int> CountWordsDFA(string text);

        public Dictionary<string, int> CountWordsDFAConcurrent(string text);
    }
}
