using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Blackbird.Applications.Sdk.Common;
using Blackbird.Applications.Sdk.Common.Files;
using Newtonsoft.Json;

namespace Apps.Widn.Models.Responses
{

    public class QualityResponse
    {
        [Display("Score")]
        public double Score { get; set; }
    }
    public class QualityEvaluate
    {
        [JsonProperty("segments")]
        [Display("Segments")]
        public IEnumerable<SegmentQuality> Segments { get; set; }

        [JsonProperty("score")]
        [Display("Score")]
        public double? Score { get; set; }
    }
    public class SegmentQuality
    {
        [JsonProperty("score")]
        [Display("Segment score")]
        public double? Score { get; set; }

        [JsonProperty("mqmScore")]
        [Display("MQM score")]
        public double? MqmScore { get; set; }

        [JsonProperty("errorSpans")]
        [Display("Error spans")]
        public IEnumerable<ErrorSpan>? ErrorSpans { get; set; }
    }

    public class ErrorSpan
    {
        [JsonProperty("text")]
        [Display("Text")]
        public string? Text { get; set; }

        [JsonProperty("confidence")]
        [Display("Confidence")]
        public double? Confidence { get; set; }

        [JsonProperty("severity")]
        [Display("Severity")]
        public string? Severity { get; set; }

        [JsonProperty("start")]
        [Display("Start")]
        public int? Start { get; set; }

        [JsonProperty("end")]
        [Display("End")]
        public int? End { get; set; }
    }
}
