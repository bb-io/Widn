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

        //var (glossaryEntries, glossaryTitle) = fileExtension switch
        //{
        //    ".tbx" => await GetEntriesFromTbx(request, glossaryStream),
        //    ".csv" => GetEntriesFromCsv(request, glossaryStream),
        //    ".tsv" => GetEntriesFromTsv(request, glossaryStream),
        //    _ => throw new Exception($"Glossary format not supported ({fileExtension})." +
        //                             "Supported file extensions include .tbx, .csv & .tsv")
        //};

        //await Client.ExecuteWithErrorHandling(requestGlossaryData);
    }
}

