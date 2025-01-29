using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Apps.Widn.Models.Responses
{
    public class FileTranslationStatusResponse
    {
        public string Status { get; set; }
        public double StatusPercentage { get; set; }
        public string Error { get; set; }
    }
}
