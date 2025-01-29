using Blackbird.Applications.Sdk.Common;
using Blackbird.Applications.Sdk.Common.Files;

namespace Apps.Widn.Models.Responses
{
    public class FileTranslationResponse
    {
        [Display("Translated file")]
        public FileReference File { get; set; }
    }
}
