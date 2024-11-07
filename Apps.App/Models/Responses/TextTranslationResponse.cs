using Blackbird.Applications.Sdk.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Apps.App.Models.Responses
{
    public class TextTranslationResponse
    {
        [Display("Target text")]
        public string TargetText { get; set; }

        [Display("Input characters")]
        public int InputCharacters { get; set; }

        [Display("Input tokens")]
        public int InputTokens { get; set; }

        [Display("Output tokens")]
        public int OutputTokens { get; set; }
    }
}
