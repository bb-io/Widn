using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Apps.Widn.Dtos
{
    public class TextTranslation
    {
        public List<string> TargetText { get; set; }
        public int InputCharacteres { get; set; }
        public int InputTokens { get; set; }
        public int OutputTokens { get; set; }
    }
}
