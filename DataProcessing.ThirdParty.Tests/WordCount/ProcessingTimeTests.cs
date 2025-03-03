using DataProcessing.ThirdParty.Tests.Models;
using System.Diagnostics;
using Xunit.Abstractions;

namespace DataProcessing.ThirdParty.Tests.WordCount
{
    public class ProcessingTimeTests
    {
        private readonly ITestOutputHelper _output;
        private Lazy<string> _englishDictionary = new Lazy<string>(() => File.ReadAllText("Resources/Dictionary of English words.txt"));
        private Lazy<string> _mobyDick = new Lazy<string>(() => File.ReadAllText("Resources/Moby-Dick.txt"));
        private Lazy<string> _bible = new Lazy<string>(() => File.ReadAllText("Resources/bible.txt"));
        private Lazy<string> _largeGeneratedFile = new Lazy<string>(() => File.ReadAllText("LargeResources/LargeGeneratedFile.txt"));

        public ProcessingTimeTests(ITestOutputHelper output)
        {
            _output = output;
        }

        [Theory]
        [ClassData(typeof(ThirdPartiesTestData))]
        public void EnglishDictionaryWords(Func<string, Dictionary<string, int>> methodToTest, string info)
        {
            _output.WriteLine($"Testing:\n {info}");

            var text = _englishDictionary.Value;

            var stopwatch = Stopwatch.StartNew();
            var actual = methodToTest(text);
            stopwatch.Stop();

            _output.WriteLine($"Method executed in: {stopwatch.ElapsedMilliseconds} ms");
            Assert.True(stopwatch.ElapsedMilliseconds < 1000, "Test took too long to execute.");
        }

        [Theory]
        [ClassData(typeof(ThirdPartiesTestData))]
        public void MobyDick(Func<string, Dictionary<string, int>> methodToTest, string info)
        {
            _output.WriteLine($"Testing:\n {info}");

            var text = _mobyDick.Value;

            var stopwatch = Stopwatch.StartNew();
            var actual = methodToTest(text);
            stopwatch.Stop();

            _output.WriteLine($"Method executed in: {stopwatch.ElapsedMilliseconds} ms");
            Assert.True(stopwatch.ElapsedMilliseconds < 1000, "Test took too long to execute.");
        }

        [Theory]
        [ClassData(typeof(ThirdPartiesTestData))]
        public void Bible(Func<string, Dictionary<string, int>> methodToTest, string info)
        {
            _output.WriteLine($"Testing:\n {info}");

            var text = _bible.Value;

            var stopwatch = Stopwatch.StartNew();
            var actual = methodToTest(text);
            stopwatch.Stop();

            _output.WriteLine($"Method executed in: {stopwatch.ElapsedMilliseconds} ms");
            Assert.True(stopwatch.ElapsedMilliseconds < 1000, "Test took too long to execute.");
        }

        [Theory]
        [ClassData(typeof(ThirdPartiesTestData))]
        public void LargeGeneratedFile(Func<string, Dictionary<string, int>> methodToTest, string info)
        {
            _output.WriteLine($"Testing:\n {info}");

            var text = _largeGeneratedFile.Value;

            var stopwatch = Stopwatch.StartNew();
            var actual = methodToTest(text);
            stopwatch.Stop();

            _output.WriteLine($"Method executed in: {stopwatch.ElapsedMilliseconds} ms");
            Assert.True(stopwatch.ElapsedMilliseconds < 1000, "Test took too long to execute.");
        }

        [Theory]
        [ClassData(typeof(ThirdPartiesWithResoucesTestData))]
        public void Resources(Func<string, Dictionary<string, int>> methodToTest, string info, string text, string resourceName)
        {
            _output.WriteLine($"Testing:\n {info}\nOn resource: {resourceName}");           

            var stopwatch = Stopwatch.StartNew();
            var actual = methodToTest(text);
            stopwatch.Stop();

            _output.WriteLine($"Method executed in: {stopwatch.ElapsedMilliseconds} ms");
            Assert.True(stopwatch.ElapsedMilliseconds < 1000, "Test took too long to execute.");
        }

        [Theory]
        [ClassData(typeof(ThirdPartiesWithLargeResoucesTestData))]
        public void LargeResources(Func<string, Dictionary<string, int>> methodToTest, string info, string text, string resourceName)
        {
            _output.WriteLine($"Testing:\n {info}\nOn resource: {resourceName}");

            var stopwatch = Stopwatch.StartNew();
            var actual = methodToTest(text);
            stopwatch.Stop();

            _output.WriteLine($"Method executed in: {stopwatch.ElapsedMilliseconds} ms");
            Assert.True(stopwatch.ElapsedMilliseconds < 1000, "Test took too long to execute.");
        }
    }
}
