using Blackbird.Applications.Sdk.Common;
using Blackbird.Applications.Sdk.Common.Files;

namespace Apps.Widn.Models.Requests;
public class FileRequest
{
    [Display("File")]
    public FileReference File { get; set; }
}
