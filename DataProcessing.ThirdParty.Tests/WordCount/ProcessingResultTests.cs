using DataProcessing.ThirdParty.Tests.Models;
using Xunit.Abstractions;
using System.Diagnostics;

namespace DataProcessing.ThirdParty.Tests.WordCount
{
    public class ProcessingResultTests
    {
        private readonly ITestOutputHelper _output;
        private Lazy<string> _englishDictionary = new Lazy<string>(() => File.ReadAllText("Resources/Dictionary of English words.txt"));

        public ProcessingResultTests(ITestOutputHelper output)
        {
            _output = output;
        }

        [Theory]
        [ClassData(typeof(ThirdPartiesTestData))]
        public void EnglishDictionaryWords(Func<string, Dictionary<string, int>> methodToTest, string info)
        {
            _output.WriteLine($"Testing:\n {info}");           

            var text = _englishDictionary.Value;
            var expected = text.Split("\n").ToDictionary(i => i.Trim(), i => 1);

            var actual = methodToTest(text);

            Assert.Equal(expected, actual);
        }

        [Theory]
        [ClassData(typeof(ThirdPartiesTestData))]
        public void SingleWord(Func<string, Dictionary<string, int>> methodToTest, string info)
        {
            _output.WriteLine($"Testing:\n {info}");

            var text = "Hello";
            var expected = new Dictionary<string, int>
            {
                { "hello", 1 }
            };

            var actual = methodToTest(text);

            Assert.Equal(expected.Count, actual.Count);
            Assert.True(!expected.Except(actual).Any());
        }

        [Theory]
        [ClassData(typeof(ThirdPartiesTestData))]
        public void MultipleWords(Func<string, Dictionary<string, int>> methodToTest, string info)
        {
            _output.WriteLine($"Testing:\n {info}");

            var text = "Go do that thing that you do so well \nI play football well";
            var expected = new Dictionary<string, int>
            {
                { "go", 1 },
                { "do", 2 },
                { "that", 2 },
                { "thing", 1 },
                { "you", 1 },
                { "so", 1 },
                { "well", 2 },
                { "i", 1 },
                { "play", 1 },
                { "football", 1 }
            };

            var actual = methodToTest(text);

            Assert.Equal(expected.Count, actual.Count);
            Assert.True(!expected.Except(actual).Any());
        }

        [Theory]
        [ClassData(typeof(ThirdPartiesTestData))]
        public void EmptyString(Func<string, Dictionary<string, int>> methodToTest, string info)
        {
            _output.WriteLine($"Testing:\n {info}");

            var text = "";
            var expected = new Dictionary<string, int>();

            var actual = methodToTest(text);

            Assert.Equal(expected.Count, actual.Count);
            Assert.True(!expected.Except(actual).Any());
        }

        [Theory]
        [ClassData(typeof(ThirdPartiesTestData))]
        public void MixedCase(Func<string, Dictionary<string, int>> methodToTest, string info)
        {
            _output.WriteLine($"Testing:\n {info}");

            var text = "Hello hello HeLLo";
            var expected = new Dictionary<string, int>
            {
                { "hello", 3 }
            };

            var actual = methodToTest(text);

            Assert.Equal(expected.Count, actual.Count);
            Assert.True(!expected.Except(actual).Any());
        }

        [Theory]
        [ClassData(typeof(ThirdPartiesTestData))]
        public void NonAlphanumericCharacters(Func<string, Dictionary<string, int>> methodToTest, string info)
        {
            _output.WriteLine($"Testing:\n {info}");

            var text = "Hello, world! This is a test. Hello again!";
            var expected = new Dictionary<string, int>
            {
                { "hello", 2 },
                { "world", 1 },
                { "this", 1 },
                { "is", 1 },
                { "a", 1 },
                { "test", 1 },
                { "again", 1 }
            };

            var actual = methodToTest(text);

            Assert.Equal(expected.Count, actual.Count);
            Assert.True(!expected.Except(actual).Any());
        }

        [Theory]
        [ClassData(typeof(ThirdPartiesTestData))]
        public void Numbers(Func<string, Dictionary<string, int>> methodToTest, string info)
        {
            _output.WriteLine($"Testing:\n {info}");

            var text = "123 456 123";
            var expected = new Dictionary<string, int>
            {
                { "123", 2 },
                { "456", 1 }
            };

            var actual = methodToTest(text);

            Assert.Equal(expected.Count, actual.Count);
            Assert.True(!expected.Except(actual).Any());
        }

        [Theory]
        [ClassData(typeof(ThirdPartiesTestData))]
        public void SpecialCharacters(Func<string, Dictionary<string, int>> methodToTest, string info)
        {
            _output.WriteLine($"Testing:\n {info}");

            var text = "@hello #world hello!";
            var expected = new Dictionary<string, int>
            {
                { "hello", 2 },
                { "world", 1 }
            };
           
            var actual = methodToTest(text);

            Assert.Equal(expected.Count, actual.Count);
            Assert.True(!expected.Except(actual).Any());
        }

        [Theory]
        [ClassData(typeof(ThirdPartiesTestData))]
        public void UnicodeCharacters(Func<string, Dictionary<string, int>> methodToTest, string info)
        {
            _output.WriteLine($"Testing:\n {info}");

            var text = "hello привіт 你好";
            var expected = new Dictionary<string, int>
            {
                { "hello", 1 },
                { "привіт", 1 },
                { "你好", 1 }
            };
           
            var actual = methodToTest(text);
            
            Assert.Equal(expected.Count, actual.Count);
            Assert.True(!expected.Except(actual).Any());
        }

    }
}
