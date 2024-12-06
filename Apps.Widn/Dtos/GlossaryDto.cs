using Blackbird.Applications.Sdk.Common;

namespace Apps.Widn.Dtos;
public class GlossaryDto
{
    public string Id { get; set; }
    public string Name { get; set; }

    [Display("Source locale")]
    public string SourceLocale { get; set; }

    [Display("Target locale")]
    public string TargetLocale { get; set; }
}

