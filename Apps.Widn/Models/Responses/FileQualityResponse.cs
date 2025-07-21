using Blackbird.Applications.Sdk.Common;
using Blackbird.Applications.Sdk.Common.Files;
using Blackbird.Applications.SDK.Blueprints.Interfaces.Review;

namespace Apps.Widn.Models.Responses
{
    public class FileQualityResponse : IReviewFileOutput
    {
        [Display("Reviewed file")]
        public FileReference File { get; set; }

        [Display("Total segments processed")]
        public int TotalSegmentsProcessed { get; set; }

        [Display("Total segments finalized")]
        public int TotalSegmentsFinalized { get; set; }

        [Display("Total segments under threshold")]
        public int TotalSegmentsUnderThreshhold { get; set; }

        [Display("Average score")]
        public float AverageMetric { get; set; }

        [Display("Percentage segments under threshold")]
        public float PercentageSegmentsUnderThreshhold { get; set; }
    }
}
