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

        [BlueprintActionDefinition(BlueprintAction.ReviewFile)]
        [Action("Review", Description = "(NEW) Estimates the quality of a translation from an XLIFF file")]
        public async Task<FileQualityResponse> ReviewFile([ActionParameter] ReviewFileRequest input)
        {
            if (input.File == null)
                throw new PluginMisconfigurationException("XLIFF file cannot be null.");

            using var stream = await _fileManagementClient.DownloadAsync(input.File);
            var content = await Transformation.Parse(stream, input.File.Name);

            var segments = content.GetSegments()
                .Where(s => !s.IsIgnorbale && s.State == SegmentState.Translated)
                .ToList();

            async Task<IEnumerable<float>> BatchReview(IEnumerable<Segment> batch)
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
                return resp.Segments.Select(s => Convert.ToSingle(s.Score ?? 0));
            }

            if (!segments.Any())
            {
                return new FileQualityResponse
                {
                    File = input.File, 
                    TotalSegmentsProcessed = 0,
                    TotalSegmentsFinalized = 0,
                    TotalSegmentsUnderThreshhold = 0,
                    AverageMetric = 0f,
                    PercentageSegmentsUnderThreshhold = 0f
                };
            }

            var segmentScores = await segments.Batch(50).Process(BatchReview);

            var finalizedSegmentsCount = 0;
            var riskySegmentsCount = 0;
            var allScores = new List<float>();

            foreach (var (segment, score) in segmentScores)
            {
                allScores.Add(score);
                if (input.ScoreThreshold.HasValue && score >= Convert.ToSingle(input.ScoreThreshold.Value))
                {
                    segment.State = SegmentState.Reviewed;
                    finalizedSegmentsCount++;
                }
                else
                {
                    riskySegmentsCount++;
                }
            }

            var updatedStream = content.Serialize().ToStream();
            var updatedFile = await _fileManagementClient.UploadAsync(
                updatedStream,
                input.File.ContentType ?? "application/xliff+xml",
                input.File.Name);

            var (total, finalized, under, average, percentUnder) = ComputeMetrics(allScores, input.ScoreThreshold);

            return new FileQualityResponse
            {
                File = updatedFile,
                TotalSegmentsProcessed = total,
                TotalSegmentsFinalized = finalized,
                TotalSegmentsUnderThreshhold = under,
                AverageMetric = average,
                PercentageSegmentsUnderThreshhold = percentUnder
            };
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
