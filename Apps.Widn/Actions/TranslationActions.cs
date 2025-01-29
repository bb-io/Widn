using Apps.Widn.Dtos;
using Apps.Widn.Invocables;
using Apps.Widn.Models.Requests;
using Apps.Widn.Models.Responses;
using Blackbird.Applications.Sdk.Common;
using Blackbird.Applications.Sdk.Common.Actions;
using Blackbird.Applications.Sdk.Common.Exceptions;
using Blackbird.Applications.Sdk.Common.Files;
using Blackbird.Applications.Sdk.Common.Invocation;
using Blackbird.Applications.SDK.Extensions.FileManagement.Interfaces;
using RestSharp;
using System.Net.Mime;

namespace Apps.Widn.Actions;

[ActionList]
public class TranslationActions(InvocationContext invocationContext, IFileManagementClient fileManagementClient) : WidnInvocable(invocationContext)
{
    [Action("Translate text", Description = "Translate a text by using model")]
    public async Task<TextTranslationResponse> TranslateText([ActionParameter][Display("Source text")] string source, [ActionParameter] TranslateConfig config)
    {
        var request = new RestRequest("/translate", Method.Post);
        request.AddJsonBody(new
        {
            config,
            sourceText = new List<string> { source },
        });
        var response = await Client.ExecuteWithErrorHandling<TextTranslation>(request);

        return new TextTranslationResponse
        {
            TargetText = response.TargetText.FirstOrDefault() ?? string.Empty,
            InputCharacters = response.InputCharacteres,
            InputTokens = response.InputTokens,
            OutputTokens = response.OutputTokens,
        };
    }

    [Action("Translate file", Description = "Translate a file by using model")]
    public async Task<FileTranslationResponse> TranslateFile([ActionParameter][Display("File")] FileReference file,
        [ActionParameter] TranslateConfig config)
    {
        var supportedFormats = new[]
        {
            "csv", "dita", "ditamap", "docm", "docx", "dtd", "htm", "html", "icml",
            "idml", "json", "markdown", "md", "mif", "mqxliff", "mxliff", "odp",
            "ods", "odt", "ots", "po", "potm", "potx", "ppsm", "ppsx", "pptm",
            "pptx", "properties", "resx", "sdlxliff", "strings", "stringsdict",
            "tmx", "tsv", "vsdx", "xml", "yaml", "yml"
        };
        var fileExtension = Path.GetExtension(file.Name)?.TrimStart('.').ToLower();
        if (string.IsNullOrEmpty(fileExtension) || !supportedFormats.Contains(fileExtension))
        {
            throw new PluginMisconfigurationException($"Unsupported file format: '{fileExtension}'. Supported formats are: {string.Join(", ", supportedFormats)}");
        }

        using var inputFileStream = await fileManagementClient.DownloadAsync(file);
        using var memoryStream = new MemoryStream();
        await inputFileStream.CopyToAsync(memoryStream);
        memoryStream.Position = 0;

        var uploadRequest = new RestRequest("/translate-file", Method.Post);
        uploadRequest.AddFile("file", memoryStream.ToArray(), file.Name, file.ContentType);

        var uploadResponse =
        await Client.ExecuteWithErrorHandling<WidnFileUploadResponse>(uploadRequest);
        var fileId = uploadResponse.FileId;
        var encryptionKey = uploadResponse.EncryptionKey;

        var translateReq = new RestRequest($"/translate-file/{fileId}/translate", Method.Post);
        translateReq.AddJsonBody(new
        {
            config
        });

        await Client.ExecuteWithErrorHandling(translateReq);

        while (true)
        {
            await Task.Delay(2000);
            var statusReq = new RestRequest($"/translate-file/{fileId}", Method.Get);
            var statusRes = await Client.ExecuteWithErrorHandling<FileTranslationStatusResponse>(statusReq);

            if (statusRes.Status == "translated")
                break;

            if (statusRes.Status is "failed" or "cancelled")
                throw new PluginApplicationException(statusRes.Error);

            var otherAllowedStatusses = new List<string>() { "created", "preprocess", "translating", "rebuilding" };
            if (!otherAllowedStatusses.Contains(statusRes.Status))
                throw new Exception($"Unknown status: {statusRes.Status}");
        }

        var downloadRequest = Client.CreateDownloadRequest(fileId, encryptionKey);
        var fileReference = new FileReference(downloadRequest, file.Name, file.ContentType);

        return new FileTranslationResponse { File = fileReference };
    }
}