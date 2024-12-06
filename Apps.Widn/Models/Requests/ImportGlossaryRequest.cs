using Apps.Widn.DataSources;
using Blackbird.Applications.Sdk.Common;
using Blackbird.Applications.Sdk.Common.Dynamic;
using Blackbird.Applications.Sdk.Common.Files;

namespace Apps.Widn.Models.Requests;
public class ImportGlossaryRequest
{
    [Display("Glossary ID", Description = "Existing glossary for import")]
    [DataSource(typeof(GlossaryDataHandler))]
    public string GlossaryId { get; set; }

    [Display("Glossary file", Description = "Glossary file exported from other Blackbird apps")]
    public FileReference File { get; set; }
}

