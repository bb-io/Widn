using Apps.Widn.Actions;
using Apps.Widn.Models.Requests;
using Blackbird.Applications.Sdk.Common.Files;
using Blackbird.Applications.Sdk.Common.Invocation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Tests.Widn.Base;

namespace Tests.Widn;

[TestClass]
public class TranslateTests : TestBase
{
    [TestMethod]
    public async Task TranslateFileReturnsValues()
    {
        var action = new TranslationActions(InvocationContext, FileManager);

        var input1 = new TranslateConfig { SourceLocale = "en", TargetLocale = "pt-PT", Model = "vesuvius", Tone = "formal" };
        var input2 = new FileReference { Name = "some.docx" };

        var result = await action.TranslateFile(new FileRequest { File = input2 }, input1);

        Assert.IsNotNull(result, "Response should not be null");
    }

    [TestMethod]
    public async Task GetQuality_ReturnsValues()
    {
        var action = new QualityActions(InvocationContext, FileManager);
        var input = new QualityEvaluateRequest { SourceText = "Dogs are loyal companions who bring joy and love into our lives.", 
            TargetText = "Los perros son compañeros leales que traen alegría y amor a nuestras vidas.", ReferenceText = "Hi" };
        var result = await action.GetQuality(input);

        var firstSegment = result.Segments.FirstOrDefault();
        Console.WriteLine($"Segment score: {firstSegment.Score}");
        Assert.IsNotNull(result);
    }
}
