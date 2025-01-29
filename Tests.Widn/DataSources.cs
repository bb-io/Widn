using Apps.Widn.Actions;
using Apps.Widn.DataSources;
using Apps.Widn.Models.Requests;
using Blackbird.Applications.Sdk.Common.Dynamic;
using Blackbird.Applications.Sdk.Common.Files;
using Tests.Widn.Base;

namespace Tests.Widn
{
    [TestClass]
    public class DataSources : TestBase
    {
        [TestMethod]
        public async Task DictionaryDataHandlerReturnsValues()
        {
            // Arrange
            var handler = new GlossaryDataHandler(InvocationContext);

            // Act
            var response = await handler.GetDataAsync(new DataSourceContext { SearchString = "" }, CancellationToken.None);

            // Assert
            Assert.IsNotNull(response, "Response should not be null");
            Assert.IsTrue(response.Any(), "Response should contain at least one item");

            foreach (var item in response) 
            {
                Console.WriteLine($"{item.Value}: {item.DisplayName}");
            }
        }

        [TestMethod]
        public async Task TranslateFileReturnsValues()
        {
            var action = new TranslationActions(InvocationContext,FileManager);

            var input1 = new TranslateConfig { SourceLocale = "en", TargetLocale = "uk", Model= "vesuvius", Tone="formal" };
            var input2 = new FileReference { Name= "some.docx"};

            var result = await action.TranslateFile(input2,input1);

            Assert.IsNotNull(result, "Response should not be null");
        }
    }
}
