using Apps.Widn.Dtos;
using Apps.Widn.Invocables;
using Apps.Widn.Models.Requests;
using Apps.Widn.Models.Responses;
using Blackbird.Applications.Sdk.Common;
using Blackbird.Applications.Sdk.Common.Actions;
using Blackbird.Applications.Sdk.Common.Exceptions;
using Blackbird.Applications.Sdk.Common.Files;
using Blackbird.Applications.Sdk.Common.Invocation;
using Blackbird.Applications.SDK.Blueprints;
using Blackbird.Applications.SDK.Extensions.FileManagement.Interfaces;
using Blackbird.Filters.Constants;
using Blackbird.Filters.Enums;
using Blackbird.Filters.Extensions;
using Blackbird.Filters.Transformations;
using RestSharp;
using System.Net.Mime;

namespace Apps.Widn.Actions;

[ActionList]
public class TranslationActions(InvocationContext invocationContext, IFileManagementClient fileManagementClient) : WidnInvocable(invocationContext)
{
    [BlueprintActionDefinition(BlueprintAction.TranslateText)]
    [Action("Translate text", Description = "Translates a text given the selected model")]
    public async Task<TextTranslationResponse> TranslateText([ActionParameter] TranslateTextRequest input)
    {
        if (string.IsNullOrWhiteSpace(input.Text))
            throw new PluginMisconfigurationException("Source text cannot be null or empty.");
        if (string.IsNullOrWhiteSpace(input.SourceLocale))
            throw new PluginMisconfigurationException("Source locale cannot be null or empty.");
        if (string.IsNullOrWhiteSpace(input.TargetLanguage))
            throw new PluginMisconfigurationException("Target locale cannot be null or empty.");

        var config = new Dictionary<string, object>
        {
            ["sourceLocale"] = input.SourceLocale,
            ["targetLocale"] = input.TargetLanguage
        };
        if (!string.IsNullOrWhiteSpace(input.Model))
            config["model"] = input.Model;
        if (!string.IsNullOrWhiteSpace(input.Tone))
            config["tone"] = input.Tone;
        if (!string.IsNullOrWhiteSpace(input.Instructions))
            config["instructions"] = input.Instructions;
        if (!string.IsNullOrWhiteSpace(input.GlossaryId))
            config["glossaryId"] = input.GlossaryId;
        if (input.MaxTokens.HasValue)
            config["maxTokens"] = input.MaxTokens.Value;

        var body = new
        {
            config,
            sourceText = new[] { input.Text }
        };

        var request = new RestRequest("/translate", Method.Post)
            .AddJsonBody(body);

        var response = await Client.ExecuteWithErrorHandling<TextTranslation>(request);

        return new TextTranslationResponse
        {
            TranslatedText = response.TargetText.FirstOrDefault() ?? string.Empty,
            InputCharacters = response.InputCharacteres,
            InputTokens = response.InputTokens,
            OutputTokens = response.OutputTokens
        };
    }

    [BlueprintActionDefinition(BlueprintAction.TranslateFile)]
    [Action("Translate", Description = "Translate a file given the selected model")]
    public async Task<FileTranslationResponse> Translate([ActionParameter] TranslateFileRequest input)
    {
        var strategy = input.FileTranslationStrategy?.ToLowerInvariant() ?? "blackbird";

        if (strategy == "widn")
        {
            return await TranslateWithWidnNative(input);
        }
        else // "blackbird"
        {
            try
            {
                using var stream = await fileManagementClient.DownloadAsync(input.File);
                var content = await Transformation.Parse(stream, input.File.Name);

                return await HandleInteroperableTransformation(content, input);
            }
            catch (Exception e) when (e.Message.Contains("not supported"))
            {
                throw new PluginMisconfigurationException(
                    "The file format is not supported by the Blackbird interoperable setting. " +
                    "Try setting the file translation strategy to Widn native.");
            }
        }
    }


    [Action("Translate file", Description = "Translate a file given the selected model")]
    public async Task<FileTranslationResponse> TranslateFile([ActionParameter] FileRequest fileRequest,
        [ActionParameter] TranslateConfig config)
    {
        var file = fileRequest.File;
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

        var downloadReq = new RestRequest($"/translate-file/{fileId}/download", Method.Get);
        downloadReq.AddQueryParameter("encryptionKey", encryptionKey);
        var translatedFileBytes = Client.DownloadData(downloadReq);

        using var outputStream = new MemoryStream(translatedFileBytes);
        var uploadedFile = await fileManagementClient.UploadAsync(
            outputStream,
            file.ContentType,
            file.Name
        );

        return new FileTranslationResponse { File = uploadedFile };
    }


    private async Task<FileTranslationResponse> HandleInteroperableTransformation(Transformation content, TranslateFileRequest input)
    {
        content.SourceLanguage ??= input.SourceLocale;
        content.TargetLanguage ??= input.TargetLanguage;

        if (content.SourceLanguage == null || content.TargetLanguage == null)
            throw new PluginMisconfigurationException("Source or target language not defined.");

        async Task<IEnumerable<(Segment, string)>> BatchTranslate(IEnumerable<Segment> batch)
        {
            var instructionsList = new List<string>();

            if (!string.IsNullOrWhiteSpace(input.Instructions))
                instructionsList.Add(input.Instructions);

            var config = new Dictionary<string, object>
            {
                ["sourceLocale"] = input.SourceLocale ?? content.SourceLanguage,
                ["targetLocale"] = input.TargetLanguage ?? content.TargetLanguage
            };
            if (!string.IsNullOrWhiteSpace(input.Model))
                config["model"] = input.Model;
            if (!string.IsNullOrWhiteSpace(input.Tone))
                config["tone"] = input.Tone;
            if (instructionsList.Any())
                config["instructions"] = string.Join(" ", instructionsList);
            if (!string.IsNullOrWhiteSpace(input.GlossaryId))
                config["glossaryId"] = input.GlossaryId;
            if (input.MaxTokens.HasValue)
                config["maxTokens"] = input.MaxTokens.Value;

            var body = new
            {
                config,
                sourceText = batch.Select(s => s.GetSource()).ToArray()
            };

            var request = new RestRequest("/translate", Method.Post).AddJsonBody(body);
            var response = await Client.ExecuteWithErrorHandling<TextTranslation>(request);

            return batch.Zip(response.TargetText, (segment, translatedText) => (segment, translatedText));
        }

        var segments = content.GetSegments()
            .Where(s => !s.IsIgnorbale && (s.State == null || s.State == SegmentState.Initial))
            .ToList();

        if (!segments.Any())
            throw new PluginMisconfigurationException("No translatable segments found.");

        var segmentTranslations = new List<(Segment, string)>();

        foreach (var batch in segments.Batch(100))
        {
            var batchResults = await BatchTranslate(batch);
            segmentTranslations.AddRange(batchResults);
        }


        foreach (var (segment, translatedText) in segmentTranslations)
        {
            if (!string.IsNullOrEmpty(translatedText))
            {
                segment.SetTarget(translatedText);
                segment.State = SegmentState.Translated;
            }
        }

        var mediaType = input.OutputFileHandling == "original"
            ? content.Target().OriginalMediaType
            : MediaTypes.Xliff;

        var resultStream = input.OutputFileHandling == "original"
            ? content.Target().Serialize().ToStream()
            : content.Serialize().ToStream();

        var fileName = input.OutputFileHandling == "original"
            ? input.File.Name
            : input.File.Name.EndsWith(".xliff") || input.File.Name.EndsWith(".xlf")
                ? input.File.Name
                : input.File.Name + ".xliff";

        var outFile = await fileManagementClient.UploadAsync(
            resultStream,
            mediaType,
            fileName);

        return new FileTranslationResponse { File = outFile };
    }


    private async Task<FileTranslationResponse> TranslateWithWidnNative(TranslateFileRequest input)
    {
        using var inStream = await fileManagementClient.DownloadAsync(input.File);
        using var ms = new MemoryStream();
        await inStream.CopyToAsync(ms);
        ms.Position = 0;

        string contentType = !string.IsNullOrWhiteSpace(input.File.ContentType) ? input.File.ContentType : "application/octet-stream";

        var uploadReq = new RestRequest("/translate-file", Method.Post)
            .AddFile("file", ms.ToArray(), input.File.Name, contentType);
        var uploadRes = await Client.ExecuteWithErrorHandling<WidnFileUploadResponse>(uploadReq);

        var config = new Dictionary<string, object>
        {
            ["sourceLocale"] = input.SourceLocale,
            ["targetLocale"] = input.TargetLanguage
        };
        if (!string.IsNullOrWhiteSpace(input.Model))
            config["model"] = input.Model ?? "sugarloaf-4.0";
        if (!string.IsNullOrWhiteSpace(input.Tone))
            config["tone"] = input.Tone ?? "automatic";
        if (!string.IsNullOrWhiteSpace(input.Instructions))
            config["instructions"] = input.Instructions;
        if (!string.IsNullOrWhiteSpace(input.GlossaryId))
            config["glossaryId"] = input.GlossaryId;
        if (input.MaxTokens.HasValue)
            config["maxTokens"] = input.MaxTokens.Value;


        var translateReq = new RestRequest($"/translate-file/{uploadRes.FileId}/translate", Method.Post)
            .AddJsonBody(new { config });
        var response = await Client.ExecuteWithErrorHandling(translateReq);

        while (true)
        {
            await Task.Delay(2000);
            var statusReq = new RestRequest($"/translate-file/{uploadRes.FileId}", Method.Get);
            var statusRes = await Client.ExecuteWithErrorHandling<FileTranslationStatusResponse>(statusReq);

            if (statusRes.Status == "translated")
            {
                break;
            }

            if (statusRes.Status is "failed" or "cancelled")
                throw new PluginApplicationException(statusRes.Error ?? "No error details");

            var otherAllowedStatuses = new List<string> { "created", "preprocess", "translating", "rebuilding" };
            if (!otherAllowedStatuses.Contains(statusRes.Status))
                throw new Exception($"Unknown status: {statusRes.Status}");
        }

        var downloadReq = new RestRequest($"/translate-file/{uploadRes.FileId}/download", Method.Get)
            .AddQueryParameter("encryptionKey", uploadRes.EncryptionKey);
        var bytes = Client.DownloadData(downloadReq);
        if (bytes == null || bytes.Length == 0)
        {
            throw new PluginApplicationException("Failed to download translated file");
        }

        using var outStream = new MemoryStream(bytes);
        var resultFile = await fileManagementClient.UploadAsync(
            outStream,
            contentType,
            input.File.Name);

        return new FileTranslationResponse { File = resultFile };
    }
}