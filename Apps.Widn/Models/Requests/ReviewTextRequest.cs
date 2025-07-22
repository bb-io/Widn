using Apps.Widn.DataSources.Enums;
using Blackbird.Applications.Sdk.Common;
using Blackbird.Applications.Sdk.Common.Dictionaries;
using Blackbird.Applications.SDK.Blueprints.Interfaces.Review;

namespace Apps.Widn.Models.Requests
{
    public class ReviewTextRequest : IReviewTextInput
    {
        [Display("Source text")]
        public string SourceText { get; set; }

        [Display("Target text")]
        public string TargetText { get; set; }

        [Display("Reference text", Description = "Use this input if you need to evaluate the text")]
        public string? ReferenceText { get; set; }

        [StaticDataSource(typeof(EstimateModelDataHandler))]
        [Display("Model", Description = "Choose model input if you need to estimate the text")]
        public string? Model { get; set; }

        [DefinitionIgnore]
        public string SourceLanguage { get; set; }

        [DefinitionIgnore]
        public string? TargetLanguage { get; set; }
    }
}
