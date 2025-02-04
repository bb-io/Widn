using System.Text.Json.Serialization;
using Apps.Widn.DataSources;
using Apps.Widn.DataSources.Enums;
using Blackbird.Applications.Sdk.Common;
using Blackbird.Applications.Sdk.Common.Dictionaries;
using Blackbird.Applications.Sdk.Common.Dynamic;

namespace Apps.Widn.Models.Requests
{
    public class TranslateConfig
    {
        [Display("Source locale")]
        [DataSource(typeof(LanguageDataHandler))]
        public string SourceLocale { get; set; }

        [Display("Target locale")]
        [DataSource(typeof(LanguageDataHandler))]
        public string TargetLocale { get; set; }

        [Display("Tone")]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        [StaticDataSource(typeof(ToneDataHandler))]
        public string? Tone { get; set; }

        [Display("Model")]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        [StaticDataSource(typeof(ModelDataHandler))]
        public string Model { get; set; }

        [Display("Instructions")]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public string? Instructions { get; set; }

        [Display("Glossary ID")]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public string? GlossaryId { get; set; }

        // TODO
        //[Display("Glossary file")]
        //public FileReference Glossary { get; set; }

        [Display("Max tokens")]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public double? MaxTokens { get; set; }
    }
}
