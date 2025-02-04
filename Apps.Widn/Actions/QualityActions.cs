﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Xml.Linq;
using Apps.Widn.Api;
using Apps.Widn.Invocables;
using Apps.Widn.Models.Requests;
using Apps.Widn.Models.Responses;
using Blackbird.Applications.Sdk.Common;
using Blackbird.Applications.Sdk.Common.Actions;
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

        [Action("Estimate quality", Description = "Evaluates the quality of a translation")]
        public async Task<QualityEvaluateResponse> GetQuality([ActionParameter] QualityEvaluateRequest input)
        {
            if (string.IsNullOrWhiteSpace(input.SourceText))
                throw new PluginMisconfigurationException("Source Text cannot be null or empty. Please check your input");

            if (string.IsNullOrWhiteSpace(input.TargetText))
                throw new PluginMisconfigurationException("Target Text cannot be null or empty. Please check your input");

            if (string.IsNullOrWhiteSpace(input.ReferenceText))
                throw new PluginMisconfigurationException("Reference Text cannot be null or empty. Please check your input");

            var requestBody = new
            {
                segments = new[]
                {
                    new
                    {
                        sourceText = input.SourceText,
                        targetText = input.TargetText,
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
            return new QualityEvaluateResponse { Score = finalScore };
        }


        [Action("Estimate XLIFF Quality", Description = "Evaluates the quality of a translation from an XLIFF file")]
        public async Task<QualityEvaluateResponse> EstimateQualityXliff([ActionParameter] QualityEvaluateXliffRequest input)
        {
            if (input.File == null)
                throw new PluginMisconfigurationException("XLIFF file cannot be null. Please provide a valid file.");
            if (string.IsNullOrWhiteSpace(input.ReferenceText))
                throw new PluginMisconfigurationException("Reference Text cannot be null or empty. Please check your input.");

            var fileStream = await _fileManagementClient.DownloadAsync(input.File);

            var segments = ExtractSegmentsFromXliff(fileStream);
            if (segments == null || !segments.Any())
                throw new PluginMisconfigurationException("No segments found in the provided XLIFF file.");

            var requestBody = new
            {
                segments = segments.Select(segment => new
                {
                    sourceText = segment.Source,
                    targetText = segment.Target,
                    referenceText = input.ReferenceText
                }).ToArray(),
                model = "xcomet-xl"
            };

            var restRequest = new RestRequest("/quality/evaluate", Method.Post);
            restRequest.AddJsonBody(requestBody);

            var response = await Client.ExecuteWithErrorHandling<QualityEvaluate>(restRequest);

            double finalScore = response.Segments?.Average(s => s.Score) ?? 0;

            fileStream.Position = 0;
            string fileContent;
            using (var reader = new StreamReader(fileStream, Encoding.UTF8, detectEncodingFromByteOrderMarks: true, leaveOpen: true))
            {
                fileContent = reader.ReadToEnd();
            }
            var modifiedFileContent = fileContent.Replace("<xliff", $"<xliff averageScore=\"{finalScore}\"");
            var modifiedFileStream = new MemoryStream(Encoding.UTF8.GetBytes(modifiedFileContent));
            var fileReference = await _fileManagementClient.UploadAsync(modifiedFileStream, "text/xml", input.File.Name);

            return new QualityEvaluateResponse
            {
                Score = finalScore,
                File = fileReference
            };
        }

        private List<TranslationUnit> ExtractSegmentsFromXliff(Stream inputStream)
        {
            var segments = new List<TranslationUnit>();

            using (var reader = new StreamReader(inputStream, Encoding.UTF8, detectEncodingFromByteOrderMarks: true, leaveOpen: true))
            {
                var xliffDocument = XDocument.Load(reader);

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

        private class TranslationUnit
        {
            public string Source { get; set; }
            public string Target { get; set; }
        }

    }
}
