using Apps.App.DataSources.Enums;
using Blackbird.Applications.Sdk.Common;
using Blackbird.Applications.Sdk.Common.Dictionaries;
using Blackbird.Applications.Sdk.Common.Files;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Apps.App.Models.Requests
{
    public class TranslateConfig
    {
        [Display("Source locale")]
        public string SourceLocale { get; set; }

        [Display("Target locale")]
        public string TargetLocale { get; set; }

        [Display("Tone")]
        [StaticDataSource(typeof(ToneDataHandler))]
        public string? Tone { get; set; }

        [Display("Model")]
        [StaticDataSource(typeof(ModelDataHandler))]
        public string? Model { get; set; }

        [Display("Instructions")]
        public string? Instructions { get; set; }

        [Display("Glossary ID")]
        public string? GlossaryId { get; set; }

        // TODO
        //[Display("Glossary file")]
        //public FileReference Glossary { get; set; }

        [Display("Max tokens")]
        public double? MaxTokens { get; set; }
    }
}
