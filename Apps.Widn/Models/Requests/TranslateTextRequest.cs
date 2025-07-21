using Apps.Widn.DataSources.Enums;
using Apps.Widn.DataSources;
using Blackbird.Applications.Sdk.Common.Dictionaries;
using Blackbird.Applications.Sdk.Common.Dynamic;
using Blackbird.Applications.SDK.Blueprints.Interfaces.Translate;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using Blackbird.Applications.Sdk.Common;

namespace Apps.Widn.Models.Requests
{
    public class TranslateTextRequest : ITranslateTextInput
    {
        [Display("Source text")]
        public string Text { get; set; }

        [Display("Source locale")]
        [DataSource(typeof(LanguageDataHandler))]
        public string SourceLocale { get; set; }

        [Display("Target locale")]
        [DataSource(typeof(LanguageDataHandler))]
        public string TargetLanguage { get; set; }

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
