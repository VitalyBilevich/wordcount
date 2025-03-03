using System.Collections.Concurrent;
using System.Text;
using TextProcessor.Enums;

namespace TextProcessor
{
    public class WordCounter : IWordCounter
    {
        public Dictionary<string, int> CountWords(string text)
        {
            if (string.IsNullOrWhiteSpace(text))
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

        // DFA (Deterministic Finite Automaton). Move between states based on input symbols (IsLetterOrDigit).
        // O(n) Complexity time 
        // O(n) Auxiliary space 
        public Dictionary<string, int> CountWordsDFA(string text)
        {
            return ProcessChunk(text, 0, text.Length);
        }

        // MapReduce        
        public Dictionary<string, int> CountWordsDFAConcurrent(string text)
        {
            var wordCounts = new ConcurrentDictionary<string, int>();
            var chunkSize = text.Length / Environment.ProcessorCount;
            var chunkResults = new ConcurrentBag<Dictionary<string, int>>();

            // Map
            Parallel.For(0, Environment.ProcessorCount, i =>
            {
                var start = i * chunkSize;
                var end = (i == Environment.ProcessorCount - 1) ? text.Length : start + chunkSize;

                // Ensure we do not split words between chunks
                if (i > 0)
                {
                    while (start < text.Length && char.IsLetterOrDigit(text[start]))
                    {
                        start++;
                    }
                }
                if (end < text.Length)
                {
                    while (end < text.Length && char.IsLetterOrDigit(text[end]))
                    {
                        end++;
                    }
                }

                var localWordCounts = ProcessChunk(text, start, end);
                chunkResults.Add(localWordCounts);
            });

            // Reduce: Merge results from all chunks
            foreach (var localWordCounts in chunkResults)
            {
                foreach (var kvp in localWordCounts)
                {
                    wordCounts.AddOrUpdate(kvp.Key, kvp.Value, (key, oldValue) => oldValue + kvp.Value);
                }
            }

            return new Dictionary<string, int>(wordCounts);
        }

        // Does it deserve to be public or a separate utility class?
        // If you think it should be covered by tests, then it should be public or a class.
        // If you really don't want to make it public or class, an alternative is to make it protected virtual.
        private Dictionary<string, int> ProcessChunk(string text, int start, int end)
        {
            var wordCounts = new Dictionary<string, int>();
            var currentState = DFAState.Start;
            var currentWord = new StringBuilder();

            for (var i = start; i < end; i++)
            {
                var ch = text[i];
                switch (currentState)
                {
                    case DFAState.Start:
                        if (char.IsLetterOrDigit(ch))
                        {
                            currentState = DFAState.InWord;
                            currentWord.Append(ch);
                        }
                        else
                        {
                            currentState = DFAState.NonWord;
                        }
                        break;

                    case DFAState.InWord:
                        if (char.IsLetterOrDigit(ch))
                        {
                            currentWord.Append(ch);
                        }
                        else
                        {
                            var word = currentWord.ToString().ToLower();
                            if (wordCounts.ContainsKey(word))
                            {
                                wordCounts[word]++;
                            }
                            else
                            {
                                wordCounts[word] = 1;
                            }
                            currentWord.Clear();
                            currentState = DFAState.NonWord;
                        }
                        break;

                    case DFAState.NonWord:
                        if (char.IsLetterOrDigit(ch))
                        {
                            currentState = DFAState.InWord;
                            currentWord.Append(ch);
                        }
                        break;
                }
            }

            // Handle the last word in the chunk
            if (currentWord.Length > 0)
            {
                var word = currentWord.ToString().ToLower();
                if (wordCounts.ContainsKey(word))
                {
                    wordCounts[word]++;
                }
                else
                {
                    wordCounts[word] = 1;
                }
            }

            return wordCounts;
        }
    }
}
