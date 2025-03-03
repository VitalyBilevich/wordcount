using Newtonsoft.Json;
using System.Collections;

namespace DataProcessing.ThirdParty.Tests.Models
{
    public class ThirdPartiesTestData: IEnumerable<object[]>
    {
        public IEnumerator<object[]> GetEnumerator()
        {
            var thirdPartyConfigs = JsonConvert.DeserializeObject<IList<ThirdPartyConfig>>(File.ReadAllText("WordCount/thirdParties.json"));

            foreach (var config in thirdPartyConfigs)
            {                
                var type = Type.GetType(config.ClassName);
                if (type == null)
                    continue;
                var methodInfo = type.GetMethod(config.MethodName);
                if (methodInfo == null)
                    continue;
                object instance = null;
                if (!methodInfo.IsStatic)
                {
                    instance = Activator.CreateInstance(type);
                }
                yield return new object[]
                {
                    new Func<string, Dictionary<string, int>>(text => (Dictionary<string, int>)methodInfo.Invoke(instance, new object[] { text })),
                    $"Name: {config.Name}\n Class: {config.ClassName}\n Method: {config.MethodName}"
                };
            }
        }

        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
       
    }
}
