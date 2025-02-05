using Blackbird.Applications.Sdk.Common;
using Newtonsoft.Json;

namespace Apps.Widn.Models.Requests
{
    public class QualityEvaluateRequest
    {
        [Display("Reference text")]
        [JsonProperty("referenceText")]
        public string ReferenceText { get; set; }
    }
}
