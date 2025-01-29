using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Blackbird.Applications.Sdk.Common;

namespace Apps.Widn.Models.Responses
{
    public class WidnFileUploadResponse
    {
        [Display("File ID")]
        public string FileId {  get; set; }

        [Display("Encryption Key")]
        public string EncryptionKey { get; set; }
    }
}
