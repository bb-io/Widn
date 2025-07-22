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
    public async Task EvaluateQuality_ReturnsValues()
    {
        var action = new QualityActions(InvocationContext, FileManager);
        var input1 = new LanguageOptions
        {
            SourceText = "Dogs are loyal companions who bring joy and love into our lives.",
            TargetText = "Los perros son compañeros leales que traen alegría y amor a nuestras vidas.",
        };
        var input2 = new QualityEvaluateRequest
        {
            ReferenceText = "Hello"
        };
        var result = await action.EvaluateQuality(input1, input2);
        Assert.IsNotNull(result);
        Console.WriteLine($"Final Score: {result.Score}");
        Assert.IsTrue(result.Score > 0);
    }

    [TestMethod]
    public async Task EstimateQuality_ReturnsValues()
    {
        var action = new QualityActions(InvocationContext, FileManager);
        var input1 = new LanguageOptions
        {
            SourceText = "Dogs are loyal companions who bring joy and love into our lives.",
            TargetText = "Los perros son compañeros leales que traen alegría y amor a nuestras vidas.",
        };

        var input2 = new EstimateModelOption {Model= "mqm-qe" };
        var result = await action.EstimateQuality(input1, input2);
        Assert.IsNotNull(result);
        Console.WriteLine($"Final Score: {result.Score}");
        Assert.IsTrue(result.Score > 0);
    }

    [TestMethod]
    public async Task ReviewEstimateOrEvaluate_ReturnsValues()
    {
        var action = new QualityActions(InvocationContext, FileManager);
        var input1 = new ReviewTextRequest
        {
            SourceText = "Dogs are loyal companions who bring joy and love into our lives.",
            TargetText = "Los perros son compañeros leales que traen alegría y amor a nuestras vidas.",
            //ReferenceText = "Los perros son amigos leales que traen alegría y amor a nuestras vidas.",
            Model = "mqm-qe" //or xcomet-xl,
        };

        var result = await action.ReviewText(input1);
        Assert.IsNotNull(result);
        Console.WriteLine($"Final Score: {result.Score}");
        Assert.IsTrue(result.Score > 0);
    }
    [TestMethod]
    public async Task ReviewEstimateFile_ReturnsValues()
    {
        var action = new QualityActions(InvocationContext, FileManager);
        var input1 = new ReviewFileRequest
        {
            ScoreThreshold = 0.7,
            File = new FileReference { Name = "contentful.html.xliff" },
            Model = "mqm-qe" //or xcomet-xl,     
        };

        var result = await action.ReviewFile(input1);

        var json = Newtonsoft.Json.JsonConvert.SerializeObject(result, Newtonsoft.Json.Formatting.Indented);
        Console.WriteLine(json);
        Assert.IsNotNull(result);
    }

    [TestMethod]
    public async Task TranslateText_ReturnsValues()
    {
        var action = new TranslationActions(InvocationContext, FileManager);
        var input1 = new TranslateTextRequest
        {
            Text = "Dogs are loyal companions who bring joy and love into our lives.",
            SourceLocale = "en",
            TargetLanguage = "pt-PT",
            Model = "anthill" ,
            
        };

        var result = await action.TranslateText(input1);

        var json = Newtonsoft.Json.JsonConvert.SerializeObject(result, Newtonsoft.Json.Formatting.Indented);
        Console.WriteLine(json);
        Assert.IsNotNull(result);
    }

    [TestMethod]
    public async Task TranslateFileNew_ReturnsValues()
    {
        var action = new TranslationActions(InvocationContext, FileManager);
        var input1 = new TranslateFileRequest
        {
            File = new FileReference { Name = "contentful.html.xliff" },
            SourceLocale = "en",
            TargetLanguage = "es",
            Model = "anthill",
            //FileTranslationStrategy = "widn" 
        };

        var result = await action.Translate(input1);

        var json = Newtonsoft.Json.JsonConvert.SerializeObject(result, Newtonsoft.Json.Formatting.Indented);
        Console.WriteLine(json);
        Assert.IsNotNull(result);
    }

    [TestMethod]
    public async Task GetQualityXLIFF_ReturnsValues()
    {
        var action = new QualityActions(InvocationContext, FileManager);
        var input = new FileRequest
        {
            File = new FileReference { Name = "translated.xliff" },
        };
        var input2 = new EstimateModelOption { Model = "mqm-qe" };
        var result = await action.EstimateQualityXliff(input, input2);
        Assert.IsNotNull(result);
        Console.WriteLine($"Final Score: {result.Score}");
        Assert.IsTrue(result.Score > 0);
    }
}
