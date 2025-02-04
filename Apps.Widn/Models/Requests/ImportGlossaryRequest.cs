using Apps.Widn.DataSources;
using Blackbird.Applications.Sdk.Common;
using Blackbird.Applications.Sdk.Common.Dynamic;
using Blackbird.Applications.Sdk.Common.Files;

namespace Apps.Widn.Models.Requests;
public class ImportGlossaryRequest
{
    [Display("Glossary file", Description = "Glossary file exported from other Blackbird apps")]
    public FileReference File { get; set; }

    [Display("New glossary name", Description = "You can override glossary name which is set in a file")]
    public string? Name { get; set; }

    [Display("Source language", Description = "Widn glossary structure is key-value. Key is your source language.\nMake sure exported glossary has this language")]
    [DataSource(typeof(LanguageDataHandler))]
    public string? SourceLanguageCode { get; set; }

    [Display("Target language", Description = "Widn glossary structure is key-value. Value is your target language.\nMake sure exported glossary has this language")]
    [DataSource(typeof(LanguageDataHandler))]
    public string? TargetLanguageCode { get; set; }

    [Display("Glossary ID", Description = "Import data to specified glossary")]
    [DataSource(typeof(GlossaryDataHandler))]
    public string? GlossaryId { get; set; }
}

