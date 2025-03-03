using System.Collections;

namespace DataProcessing.ThirdParty.Tests.Models
{
    public class ThirdPartiesWithResoucesTestData : IEnumerable<object[]>
    {
        private readonly bool _largeResources = false;
        private Lazy<IEnumerable<object[]>> _lazyData;

        public ThirdPartiesWithResoucesTestData()
        {            
            _lazyData = new Lazy<IEnumerable<object[]>>(() => GenerateData());
        }

        public ThirdPartiesWithResoucesTestData(bool largeResources): this()
        {
            _largeResources = largeResources;           
        }

        public IEnumerator<object[]> GetEnumerator()
        {
            return _lazyData.Value.GetEnumerator(); // Access the Lazy value when needed
        }

        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

        private IEnumerable<object[]> GenerateData()
        {
            var filePaths = Directory.GetFiles(_largeResources ? "LargeResources" : "Resources", "*.txt");
            var fileContents = new List<string>();
            var readFiles = filePaths.Select(async filePath => await File.ReadAllTextAsync(filePath)).ToArray();
            Task.WaitAll(readFiles);
            fileContents.AddRange(readFiles.Select(f => f.Result));

            var thirdPartiesTestData = new ThirdPartiesTestData().GetEnumerator();
            while (thirdPartiesTestData.MoveNext())
            {
                var i = 0;
                foreach (var fileContent in fileContents)
                {
                    yield return new object[]
                    {
                        thirdPartiesTestData.Current[0],
                        thirdPartiesTestData.Current[1],
                        fileContent,
                        Path.GetFileName(filePaths[i])
                    };
                    i++;
                }
            }
        }
        
    }
}
