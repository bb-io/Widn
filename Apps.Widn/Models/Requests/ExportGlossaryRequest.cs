using Apps.Widn.DataSources;
using Blackbird.Applications.Sdk.Common;
using Blackbird.Applications.Sdk.Common.Dynamic;

namespace Apps.Widn.Models.Requests;
public class ExportGlossaryRequest
{
    [Display("Glossary ID")]
    [DataSource(typeof(GlossaryDataHandler))]
    public string GlossaryId { get; set; }
}

