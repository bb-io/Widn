using Blackbird.Applications.Sdk.Common;
using Newtonsoft.Json;

namespace Apps.Widn.Models.Requests
{
    public class LanguageOptions
    {
        [Display("Source Text")]
        [JsonProperty("sourceText")]
        public string SourceText { get; set; }

        [Display("Target text")]
        [JsonProperty("targetText")]
        public string TargetText { get; set; }
    }
}
