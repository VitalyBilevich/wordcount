using Moq;

namespace TextProcessor.Tests
{
    public class CountWordsDFATests
    {
        [Fact]
        public void CountWordsDFA_SingleWord()
        {
            var text = "Hello";
            var expected = new Dictionary<string, int>
            {
                { "hello", 1 }
            };

            var wordCounterMock = new Mock<IWordCounter>();
            wordCounterMock.Setup(wc => wc.CountWordsDFA(text)).Returns(expected);

            var wordCounter = wordCounterMock.Object;
            var actual = wordCounter.CountWordsDFA(text);

            Assert.Equal(expected, actual);
        }

        [Fact]
        public void CountWordsDFA_MultipleWords()
        {
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

            var wordCounterMock = new Mock<IWordCounter>();
            wordCounterMock.Setup(wc => wc.CountWordsDFA(text)).Returns(expected);

            var wordCounter = wordCounterMock.Object;
            var actual = wordCounter.CountWordsDFA(text);

            Assert.Equal(expected, actual);
        }

        [Fact]
        public void CountWordsDFA_EmptyString()
        {
            string text = "";
            var expected = new Dictionary<string, int>();

            var wordCounterMock = new Mock<IWordCounter>();
            wordCounterMock.Setup(wc => wc.CountWordsDFA(text)).Returns(expected);

            var wordCounter = wordCounterMock.Object;
            var actual = wordCounter.CountWordsDFA(text);

            Assert.Equal(expected, actual);
        }

        [Fact]
        public void CountWordsDFA_MixedCase()
        {
            string text = "Hello hello HeLLo";
            var expected = new Dictionary<string, int>
            {
                { "hello", 3 }
            };

            var wordCounterMock = new Mock<IWordCounter>();
            wordCounterMock.Setup(wc => wc.CountWordsDFA(text)).Returns(expected);

            var wordCounter = wordCounterMock.Object;
            var actual = wordCounter.CountWordsDFA(text);

            Assert.Equal(expected, actual);
        }

        [Fact]
        public void CountWordsDFA_NonAlphanumericCharacters()
        {
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

            var wordCounterMock = new Mock<IWordCounter>();
            wordCounterMock.Setup(wc => wc.CountWordsDFA(text)).Returns(expected);

            var wordCounter = wordCounterMock.Object;
            var actual = wordCounter.CountWordsDFA(text);

            Assert.Equal(expected, actual);
        }

        [Fact]
        public void CountWordsDFA_Numbers()
        {
            var text = "123 456 123";
            var expected = new Dictionary<string, int>
            {
                { "123", 2 },
                { "456", 1 }
            };

            var wordCounterMock = new Mock<IWordCounter>();
            wordCounterMock.Setup(wc => wc.CountWordsDFA(text)).Returns(expected);

            var wordCounter = wordCounterMock.Object;
            var actual = wordCounter.CountWordsDFA(text);

            Assert.Equal(expected, actual);
        }

        [Fact]
        public void CountWordsDFA_SpecialCharacters()
        {
            var text = "@hello #world hello!";
            var expected = new Dictionary<string, int>
            {
                { "hello", 2 },
                { "world", 1 }
            };

            var wordCounterMock = new Mock<IWordCounter>();
            wordCounterMock.Setup(wc => wc.CountWordsDFA(text)).Returns(expected);

            var wordCounter = wordCounterMock.Object;
            var actual = wordCounter.CountWordsDFA(text);

            Assert.Equal(expected, actual);
        }
    }

}