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
using Blackbird.Applications.Sdk.Common.Files;
using Blackbird.Applications.SDK.Blueprints.Handlers;
using Apps.DeepL.DataSourceHandlers.Enums;

namespace Apps.Widn.Models.Requests
{
    public class TranslateFileRequest : ITranslateFileInput
    {
        [Display("File")]
        public FileReference File { get; set; }

        [Display("Source language")]
        [DataSource(typeof(LanguageDataHandler))]
        public string SourceLocale { get; set; }

        [Display("Target language")]
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

        [Display("Output file handling", Description = "Determine the format of the output file. The default Blackbird behavior is to convert to XLIFF for future steps."), StaticDataSource(typeof(ProcessFileFormatHandler))]
        public string? OutputFileHandling { get; set; }

        // TODO
        //[Display("Glossary file")]
        //public FileReference Glossary { get; set; }

        [Display("Max tokens")]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public double? MaxTokens { get; set; }

        [Display("File translation strategy", Description = "Select whether to use Widn's own file processing capabilities or use Blackbird interoperability mode"), StaticDataSource(typeof(FileTranslationStrategyHandler))]
        public string? FileTranslationStrategy { get; set; }
    }
}
