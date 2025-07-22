using Blackbird.Applications.Sdk.Common;
using Blackbird.Applications.SDK.Blueprints.Interfaces.Translate;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Apps.Widn.Models.Responses
{
    public class TextTranslationResponse: ITranslateTextOutput
    {
        [Display("Translated text")]
        public string TranslatedText { get; set; }

        [Display("Input characters")]
        public int InputCharacters { get; set; }

        [Display("Input tokens")]
        public int InputTokens { get; set; }

        [Display("Output tokens")]
        public int OutputTokens { get; set; }
    }
}
