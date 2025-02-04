using Blackbird.Applications.Sdk.Common;
using Blackbird.Applications.Sdk.Common.Files;

namespace Apps.Widn.Models.Responses
{
    public class QualityEvaluateXliffResponse
    {
        [Display("Score")]
        public double Score { get; set; }

        [Display("XLIFF file")]
        public FileReference File { get; set; }
    }
}
