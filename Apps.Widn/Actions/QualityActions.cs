using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Mime;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Xml.Linq;
using Apps.Widn.Api;
using Apps.Widn.DataSources.Enums;
using Apps.Widn.Invocables;
using Apps.Widn.Models;
using Apps.Widn.Models.Requests;
using Apps.Widn.Models.Responses;
using Blackbird.Applications.Sdk.Common;
using Blackbird.Applications.Sdk.Common.Actions;
using Blackbird.Applications.Sdk.Common.Dictionaries;
using Blackbird.Applications.Sdk.Common.Exceptions;
using Blackbird.Applications.Sdk.Common.Files;
using Blackbird.Applications.Sdk.Common.Invocation;
using Blackbird.Applications.SDK.Extensions.FileManagement.Interfaces;
using DocumentFormat.OpenXml.Office2010.ExcelAc;
using RestSharp;

namespace Apps.Widn.Actions
{
    [ActionList]
    public class QualityActions : WidnInvocable
    {
        private readonly IFileManagementClient _fileManagementClient;
        public QualityActions(InvocationContext invocationContext, IFileManagementClient fileManagementClient) : base(invocationContext)
        {
            _fileManagementClient = fileManagementClient;
        }

        [Action("Evaluate translation quality", Description = "Evaluates the quality of a translation")]
        public async Task<QualityResponse> EvaluateQuality([ActionParameter] LanguageOptions option, [ActionParameter] QualityEvaluateRequest input)
        {
            if (string.IsNullOrWhiteSpace(option.SourceText))
                throw new PluginMisconfigurationException("Source Text cannot be null or empty. Please check your input");

            if (string.IsNullOrWhiteSpace(option.TargetText))
                throw new PluginMisconfigurationException("Target Text cannot be null or empty. Please check your input");

            if (string.IsNullOrWhiteSpace(input.ReferenceText))
                throw new PluginMisconfigurationException("Reference Text cannot be null or empty. Please check your input");

            var requestBody = new
            {
                segments = new[]
                {
                    new
                    {
                        sourceText = option.SourceText,
                        targetText = option.TargetText,
                        referenceText = input.ReferenceText
                    }
                },
                model = "xcomet-xl"
            };

            var restRequest = new RestRequest("/quality/evaluate", Method.Post);
            restRequest.AddJsonBody(requestBody);
            var response = await Client.ExecuteWithErrorHandling<QualityEvaluate>(restRequest);
            var score = response.Segments;

            double finalScore = response.Segments?.FirstOrDefault()?.Score ?? 0;
            return new QualityResponse { Score = finalScore };
        }


        [Action("Estimate translation quality", Description = "Estimate the quality of a translation")]
        public async Task<QualityResponse> EstimateQuality([ActionParameter] LanguageOptions option, [ActionParameter] EstimateModelOption model)
        {
            if (string.IsNullOrWhiteSpace(option.SourceText))
                throw new PluginMisconfigurationException("Source Text cannot be null or empty. Please check your input");

            if (string.IsNullOrWhiteSpace(option.TargetText))
                throw new PluginMisconfigurationException("Target Text cannot be null or empty. Please check your input");

            if (string.IsNullOrWhiteSpace(model.Model))
                throw new PluginMisconfigurationException("Model input cannot be null or empty. Please check your input");

            var requestBody = new
            {
                segments = new[]
                {
                    new
                    {
                        sourceText = option.SourceText,
                        targetText = option.TargetText,
                    }
                },
                model = model.Model
            };

            var restRequest = new RestRequest("/quality/estimate", Method.Post);
            restRequest.AddJsonBody(requestBody);
            var response = await Client.ExecuteWithErrorHandling<QualityEvaluate>(restRequest);
            var score = response.Segments;

            double finalScore = response.Segments?.FirstOrDefault()?.Score ?? 0;
            return new QualityResponse { Score = finalScore };
        }



        [Action("Estimate XLIFF translation quality", Description = "Estimates the quality of a translation from an XLIFF file")]
        public async Task<QualityResponse> EstimateQualityXliff([ActionParameter] FileRequest input, [ActionParameter] EstimateModelOption model)
        {
            if (input.File == null)
                throw new PluginMisconfigurationException("XLIFF file cannot be null. Please provide a valid file.");

            if (string.IsNullOrWhiteSpace(input.File.Url))
                throw new PluginMisconfigurationException("File URL is missing in the provided FileReference.");

            var downloadRequest = new RestRequest(input.File.Url);
            var downloadResponse = await Client.ExecuteAsync(downloadRequest);

            if (!downloadResponse.IsSuccessStatusCode)
                throw new PluginMisconfigurationException($"Could not download your file; Code: {downloadResponse.StatusCode}");


            var fileStream = new MemoryStream(downloadResponse.RawBytes);

            var segments = ExtractSegmentsFromXliff(fileStream);
            if (segments == null || !segments.Any())
                throw new PluginMisconfigurationException("No segments found in the provided XLIFF file. Please check the input file");

            var requestBody = new
            {
                segments = segments.Select(segment => new
                {
                    sourceText = segment.Source,
                    targetText = segment.Target,
                }).ToArray(),
                model = model.Model ??"xcomet-xl"
            };

            var restRequest = new RestRequest("/quality/estimate", Method.Post);
            restRequest.AddJsonBody(requestBody);

            var response = await Client.ExecuteWithErrorHandling<QualityEvaluate>(restRequest);
            double finalScore = response.Segments?.Average(s => s.Score) ?? 0;

            return new QualityResponse
            {
                Score = finalScore
            };
        }

        private List<TranslationUnit> ExtractSegmentsFromXliff(Stream inputStream)
        {
            var segments = new List<TranslationUnit>();

            using (var reader = new StreamReader(inputStream, Encoding.UTF8, detectEncodingFromByteOrderMarks: true, leaveOpen: true))
            {
                var xliffDocument = XDocument.Load(reader);

                if (xliffDocument.Root == null || xliffDocument.Root.Name.LocalName.ToLower() != "xliff")
                {
                    throw new PluginMisconfigurationException("Invalid file format. The provided file does not appear to be a valid XLIFF file. Please check the input file");
                }

                XNamespace ns = xliffDocument.Root.GetDefaultNamespace();

                var transUnits = xliffDocument.Descendants(ns + "trans-unit");
                foreach (var tu in transUnits)
                {
                    var sourceElement = tu.Element(ns + "source");
                    var targetElement = tu.Element(ns + "target");

                    if (sourceElement != null && targetElement != null)
                    {
                        segments.Add(new TranslationUnit
                        {
                            Source = sourceElement.Value,
                            Target = targetElement.Value
                        });
                    }
                }
            }

            return segments;
        }

    }
}
