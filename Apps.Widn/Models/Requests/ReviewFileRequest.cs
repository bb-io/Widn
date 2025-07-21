using Apps.Widn.DataSources.Enums;
using Blackbird.Applications.Sdk.Common;
using Blackbird.Applications.Sdk.Common.Dictionaries;
using Blackbird.Applications.Sdk.Common.Files;
using Blackbird.Applications.SDK.Blueprints.Interfaces.Review;

namespace Apps.Widn.Models.Requests
{
    public class ReviewFileRequest : IReviewFileInput
    {
        [StaticDataSource(typeof(EstimateModelDataHandler))]
        [Display("Model", Description = "Choose model input if you need to estimate the text")]
        public string? Model { get; set; }

        [Display("File")]
        public FileReference File { get; set; }

        [Display("Score threshold")]
        public double? ScoreThreshold { get; set; }

        [DefinitionIgnore]
        public string? TargetLanguage { get; set; }

        [DefinitionIgnore]
        public string? OutputFileHandling { get; set; }
    }
}
