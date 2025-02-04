using Blackbird.Applications.Sdk.Common;
using Blackbird.Applications.Sdk.Common.Files;

namespace Apps.Widn.Models.Requests
{
    public class QualityEvaluateXliffRequest
    {
        [Display("XLIFF File")]
        public FileReference File { get; set; }

        [Display("Reference Text")]
        public string ReferenceText { get; set; }
    }
}
