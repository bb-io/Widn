using Apps.Widn.Dtos;
using Apps.Widn.Invocables;
using Apps.Widn.Models.Requests;
using Apps.Widn.Models.Responses;
using Blackbird.Applications.Sdk.Common;
using Blackbird.Applications.Sdk.Common.Actions;
using Blackbird.Applications.Sdk.Common.Invocation;
using Blackbird.Applications.Sdk.Glossaries.Utils.Converters;
using Blackbird.Applications.Sdk.Utils.Extensions.String;
using Blackbird.Applications.SDK.Extensions.FileManagement.Interfaces;
using RestSharp;
using System.Net.Mime;
using Blackbird.Applications.Sdk.Utils.Extensions.Files;
using Blackbird.Applications.Sdk.Glossaries.Utils.Dtos;
using DocumentFormat.OpenXml.Office2016.Excel;
using DocumentFormat.OpenXml.Spreadsheet;
using Blackbird.Applications.Sdk.Common.Exceptions;

namespace Apps.Widn.Actions;

[ActionList]
public class GlossaryActions : WidnInvocable
{
    private readonly IFileManagementClient _fileManagementClient;

    public GlossaryActions(InvocationContext invocationContext, IFileManagementClient fileManagementClient) : base(invocationContext)
    {
        _fileManagementClient = fileManagementClient;
    }

    [Action("Export glossary", Description = "Export glossary")]
    public async Task<ExportGlossaryResponse> ExportGlossary([ActionParameter] ExportGlossaryRequest input)
    {
        var endpointGlossaryData = $"/glossary/{input.GlossaryId}/item​";
        var requestGlossaryData = new RestRequest(endpointGlossaryData, Method.Get);
        var responseGlossaryData = await Client.Paginate<GlossaryItemDto>(requestGlossaryData);

        var endpointGlossaryDetails = $"/glossary/{input.GlossaryId}";
        var requestGlossaryDetails = new RestRequest(endpointGlossaryDetails, Method.Get);
        var responseGlossaryDetails = await Client.ExecuteWithErrorHandling<GlossaryDto>(requestGlossaryDetails);

        var conceptEntries = new List<GlossaryConceptEntry>();
        int counter = 0;
        foreach (var entry in responseGlossaryData)
        {
            var glossaryTermSection1 = new List<GlossaryTermSection> { new(entry.Term) };

            var glossaryTermSection2 = new List<GlossaryTermSection> { new(entry.Translation) };

            var languageSections = new List<GlossaryLanguageSection>
            {
                new(responseGlossaryDetails.SourceLocale, glossaryTermSection1),
                new(responseGlossaryDetails.TargetLocale, glossaryTermSection2)
            };

            conceptEntries.Add(new GlossaryConceptEntry(counter.ToString(), languageSections));
            ++counter;
        }

        var blackbirdGlossary = new Glossary(conceptEntries)
        {
            Title = responseGlossaryDetails.Name
        };
        await using var stream = blackbirdGlossary.ConvertToTbx();
        return new ExportGlossaryResponse()
        {
            File = await _fileManagementClient.UploadAsync(stream, MediaTypeNames.Application.Xml,
                $"{responseGlossaryDetails.Name}.tbx")
        };
    }

    [Action("Import glossary", Description = "Import glossary")]
    public async Task ImportGlossary([ActionParameter] ImportGlossaryRequest input)
    {
        await using var glossaryStream = await _fileManagementClient.DownloadAsync(input.File);
        var fileExtension = Path.GetExtension(input.File.Name);

        var endpointGlossary = $"/glossary/{input.GlossaryId}";
        var requestGlossaryDetails = new RestRequest(endpointGlossary, Method.Get);
        var responseGlossaryDetails = await Client.ExecuteWithErrorHandling<GlossaryDto>(requestGlossaryDetails);

        var (glossaryEntries, glossaryTitle) = fileExtension switch
        {
            ".tbx" => await GetEntriesFromTbx(input, glossaryStream),
            //".csv" => GetEntriesFromCsv(request, glossaryStream),
            //".tsv" => GetEntriesFromTsv(request, glossaryStream),
            //_ => throw new Exception($"Glossary format not supported ({fileExtension})." +
            //                         "Supported file extensions include .tbx, .csv & .tsv")
        };

        RestRequest createOrUpdateGlossary = new RestRequest();
        if (input.GlossaryId != null)
        {
            createOrUpdateGlossary = new RestRequest(endpointGlossary, Method.Put);
            createOrUpdateGlossary.AddBody(new
            {
                name = responseGlossaryDetails.Name,
                sourceLocale = responseGlossaryDetails.SourceLocale,
                targetLocale = responseGlossaryDetails.TargetLocale,
                items = glossaryEntries
            });
        }
        else if (input.SourceLanguageCode != null && input.TargetLanguageCode != null)
        {
            createOrUpdateGlossary = new RestRequest("/glossary", Method.Post);
            createOrUpdateGlossary.AddBody(new
            {
                name = input.Name ?? glossaryTitle,
                sourceLocale = input.SourceLanguageCode,
                targetLocale = input.TargetLanguageCode,
                items = glossaryEntries
            });
        }
        else
        {
            throw new PluginMisconfigurationException("Please specify existing glossary ID or source and target locales for creating new one.");
        }
        await Client.ExecuteWithErrorHandling(createOrUpdateGlossary);
    }

    private async Task<(List<GlossaryItemDto> entries, string name)> GetEntriesFromTbx(ImportGlossaryRequest request,
        Stream glossaryStream)
    {
        var blackbirdGlossary = await glossaryStream.ConvertFromTbx();
        var glossaryValues = new List<KeyValuePair<string, string>>();
        foreach (var entry in blackbirdGlossary.ConceptEntries)
        {
            var langSectionSource =
                entry.LanguageSections.FirstOrDefault(x => x.LanguageCode.ToLower() == request.SourceLanguageCode);
            if (langSectionSource is null && request.SourceLanguageCode == "en")
            {
                langSectionSource =
                entry.LanguageSections.FirstOrDefault(x => x.LanguageCode.ToLower() == "en-us" || x.LanguageCode.ToLower() == "en-gb");
            }
            var langSectionTarget =
                entry.LanguageSections.FirstOrDefault(x => x.LanguageCode.ToLower() == request.TargetLanguageCode);
            if (langSectionTarget is null)
            {
                langSectionTarget =
                entry.LanguageSections.FirstOrDefault(x => x.LanguageCode.ToLower().Substring(0, 2) == request.TargetLanguageCode.Substring(0, 2));
            }
            if (langSectionSource == null || langSectionTarget == null)
            {
                continue;
            }

            var cleanTermSource = CleanText(langSectionSource.Terms.First().Term);
            var cleanTermTarget = CleanText(langSectionTarget.Terms.First().Term);
            glossaryValues.Add(new KeyValuePair<string, string>(cleanTermSource, cleanTermTarget));
        }

        return (glossaryValues.DistinctBy(x => x.Key).Select(x => new GlossaryItemDto() { Term = x.Key, Translation = x.Value}).ToList(),
            request.Name ?? blackbirdGlossary.Title!);
    }

    //private (GlossaryEntries entries, string name) GetEntriesFromCsv(ImportGlossaryRequest request,
    //    Stream glossaryStream)
    //{
    //    var lines = new List<string>();
    //    using (StreamReader reader = new StreamReader(glossaryStream))
    //    {
    //        while (!reader.EndOfStream)
    //        {
    //            lines.Add(reader.ReadLine()!);
    //        }
    //    }

    //    var entries = new List<KeyValuePair<string, string>>();
    //    foreach (var line in lines)
    //    {
    //        entries.Add(new KeyValuePair<string, string>(line.Split(',')[0], line.Split(',')[1]));
    //    }

    //    return (new GlossaryEntries(entries.DistinctBy(x => x.Key)), request.Name ?? request.File.Name);
    //}

    //private (GlossaryEntries entries, string name) GetEntriesFromTsv(ImportGlossaryRequest request,
    //    Stream glossaryStream)
    //{
    //    var tsvLines = new List<string>();
    //    using (StreamReader reader = new StreamReader(glossaryStream))
    //    {
    //        while (!reader.EndOfStream)
    //        {
    //            tsvLines.Add(reader.ReadLine()!);
    //        }
    //    }

    //    var tsvEntries = new List<KeyValuePair<string, string>>();
    //    foreach (var line in tsvLines)
    //    {
    //        tsvEntries.Add(new KeyValuePair<string, string>(line.Split('\t')[0], line.Split('\t')[1]));
    //    }

    //    return (new GlossaryEntries(tsvEntries.DistinctBy(x => x.Key)), request.Name ?? request.File.Name);
    //}
    private string CleanText(string input)
    {
        return input.Replace("\r", "").Replace("\n", " ").Replace("\u2028", "");
    }
}

