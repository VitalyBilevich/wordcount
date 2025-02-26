namespace TextProcessor
{
    public interface IWordCounter
    {
        public Dictionary<string, int> CountWords(string text);
    }
}
