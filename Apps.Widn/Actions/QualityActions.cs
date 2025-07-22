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
using Blackbird.Applications.SDK.Blueprints;
using Blackbird.Applications.SDK.Extensions.FileManagement.Interfaces;
using Blackbird.Filters.Enums;
using Blackbird.Filters.Extensions;
using Blackbird.Filters.Transformations;
using DocumentFormat.OpenXml.EMMA;
using DocumentFormat.OpenXml.Office2010.ExcelAc;
using RestSharp;

namespace Apps.Widn.Actions
{
    [ActionList("Review")]
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

            var rawScore = response.Segments?.FirstOrDefault()?.Score ?? 0;
            float finalScore = Convert.ToSingle(rawScore);
            return new QualityResponse { Score = finalScore };
        }

        [BlueprintActionDefinition(BlueprintAction.ReviewText)]
        [Action("Review text", Description = "(NEW) Estimates or evaluates the quality of a translation")]
        public async Task<QualityResponse> ReviewText([ActionParameter] ReviewTextRequest input)
        {
            if (string.IsNullOrWhiteSpace(input.SourceText))
                throw new PluginMisconfigurationException("Source text cannot be null or empty. Please check your input and try again");
            if (string.IsNullOrWhiteSpace(input.TargetText))
                throw new PluginMisconfigurationException("Target text cannot be null or empty. Please check your input and try again");

            var (path, body) = BuildQualityRequest(input);

            var restRequest = new RestRequest(path, Method.Post);
            restRequest.AddJsonBody(body);
            var response = await Client.ExecuteWithErrorHandling<QualityEvaluate>(restRequest);

            var rawScore = response.Segments?.FirstOrDefault()?.Score ?? 0;
            return new QualityResponse { Score = Convert.ToSingle(rawScore) };
        }

        private (string path, object requestBody) BuildQualityRequest(ReviewTextRequest input)
        {
            bool doEvaluate = !string.IsNullOrWhiteSpace(input.ReferenceText);
            bool doEstimate = !string.IsNullOrWhiteSpace(input.Model);

            if (doEvaluate && doEstimate)
                throw new PluginMisconfigurationException(
                    "Please specify either Model (for estimate) or Reference text (for evaluate), not both.");
            if (!doEvaluate && !doEstimate)
                throw new PluginMisconfigurationException(
                    "You must provide either Model (for estimate) or Reference text (for evaluate).");

            if (doEstimate)
            {
                var body = new
                {
                    segments = new[]
                    {
                        new
                        {
                            sourceText = input.SourceText,
                            targetText = input.TargetText
                        }
                    },
                    model = input.Model!
                };
                return ("/quality/estimate", body);
            }
            else
            {
                var body = new
                {
                    segments = new[]
                    {
                        new
                        {
                            sourceText = input.SourceText,
                            targetText = input.TargetText,
                            referenceText = input.ReferenceText!
                        }
                    },
                    model = "xcomet-xl"
                };
                return ("/quality/evaluate", body);
            }
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

            var rawScore = response.Segments?.FirstOrDefault()?.Score ?? 0;
            float finalScore = Convert.ToSingle(rawScore);
            return new QualityResponse { Score = finalScore };
        }

        [BlueprintActionDefinition(BlueprintAction.ReviewFile)]
        [Action("Review", Description = "Estimates the quality of a translation from an XLIFF file")]
        public async Task<FileQualityResponse> ReviewFile([ActionParameter] ReviewFileRequest input)
        {
            if (input.File == null)
                throw new PluginMisconfigurationException("XLIFF file cannot be null.");

            using var stream = await _fileManagementClient.DownloadAsync(input.File);
            var content = await Transformation.Parse(stream, input.File.Name);

            var segments = content.GetSegments()
                .Where(s => !s.IsIgnorbale)
                .ToList();

            if (!segments.Any())
                throw new PluginMisconfigurationException("No segments found in the provided XLIFF file.");

            const int batchSize = 50;
            var allScores = new List<float>();

            var finalizedSegmentsCount = 0;
            var riskySegmentsCount = 0;

            foreach (var batch in segments
                .Select((seg, idx) => new { seg, idx })
                .GroupBy(x => x.idx / batchSize, x => x.seg)
                .Select(g => g.ToList()))
            {
                var requestBody = new
                {
                    segments = batch.Select(s => new
                    {
                        sourceText = s.GetSource(),
                        targetText = s.GetTarget()
                    }).ToArray(),
                    model = input.Model ?? "xcomet-xl"
                };

                var req = new RestRequest("/quality/estimate", Method.Post)
                    .AddJsonBody(requestBody);

                var resp = await Client.ExecuteWithErrorHandling<QualityEvaluate>(req);

                var segmentsList = resp.Segments.ToList();

                for (int i = 0; i < batch.Count && i < segmentsList.Count; i++)
                {
                    var score = Convert.ToSingle(segmentsList[i].Score ?? 0);
                    allScores.Add(score);

                    if (input.ScoreThreshold.HasValue && score >= input.ScoreThreshold.Value)
                    {
                        batch[i].State = SegmentState.Final;
                        finalizedSegmentsCount++;
                    }
                    else
                    {
                        riskySegmentsCount++;
                    }
                }
            }

            var updatedStream = content.Serialize().ToStream();
            var updatedFile = await _fileManagementClient.UploadAsync(
                updatedStream,
                input.File.ContentType,
                input.File.Name
            );

            var (total, finalized, under, average, percentUnder) = ComputeMetrics(allScores, input.ScoreThreshold);

            return new FileQualityResponse
            {
                TotalSegmentsProcessed = total,
                TotalSegmentsFinalized = finalized, 
                TotalSegmentsUnderThreshhold = under,
                AverageMetric = average,
                PercentageSegmentsUnderThreshhold = percentUnder
            };
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
            double rawAverage = response.Segments?.Average(s => s.Score) ?? 0;
            float finalScore = Convert.ToSingle(rawAverage);

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
                XDocument xliffDocument;
                try
                {
                    xliffDocument = XDocument.Load(reader);
                }
                catch (Exception ex)
                {
                    throw new PluginMisconfigurationException("Failed to proceed the input file. Please ensure that the file is a valid XLIFF file.");
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

        private (int Total, int Finalized, int Under, float Average, float PercentageUnder)
        ComputeMetrics(List<float> scores, double? thresholdNullable)
        {
            int total = scores.Count;
            float average = total > 0 ? scores.Average() : 0f;

            int under = 0;
            if (thresholdNullable.HasValue)
            {
                float threshold = Convert.ToSingle(thresholdNullable.Value);
                under = scores.Count(s => s < threshold);
            }

            int finalized = total - under;
            float percentUnder = total > 0 ? under * 100f / total : 0f;

            return (total, finalized, under, average, percentUnder);
        }
    }
}
