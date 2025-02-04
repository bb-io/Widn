using System.Net;
using Apps.Widn.Constants;
using Apps.Widn.Dtos;
using Blackbird.Applications.Sdk.Common.Authentication;
using Blackbird.Applications.Sdk.Common.Exceptions;
using Blackbird.Applications.Sdk.Utils.Extensions.String;
using Blackbird.Applications.Sdk.Utils.RestSharp;
using Newtonsoft.Json;
using RestSharp;

namespace Apps.Widn.Api;

public class WidnClient : BlackBirdRestClient
{
    private string apiKey;

    public WidnClient(IEnumerable<AuthenticationCredentialsProvider> authenticationCredentialsProviders) :
            base(new RestClientOptions { ThrowOnAnyError = false, BaseUrl = new Uri("https://api.widn.ai/v1") })
    {
        var apiKey = authenticationCredentialsProviders.First(x => x.KeyName == CredsNames.ApiKey).Value;
        this.AddDefaultHeader("x-api-key", apiKey);  
        // Add the X-Widn-App header
        this.AddDefaultHeader("X-Widn-App", CredsNames.BlackBird);
    }

    public HttpRequestMessage CreateDownloadRequest(string fileId, string encryptionKey)
    {
        var request = new HttpRequestMessage(HttpMethod.Get, $"https://api.widn.ai/v1/translate-file/{fileId}/download?encryptionKey={encryptionKey}");
        request.Headers.Add("x-api-key", apiKey);
        request.Headers.Add("X-Widn-App", CredsNames.BlackBird);
        return request;
    }

    protected override Exception ConfigureErrorException(RestResponse response)
    {
        try
        {
            if (response.Content is null)
            {
                return new PluginApplicationException($"Error: {response.ErrorMessage}");
            }

            var error = JsonConvert.DeserializeObject<Error>(response.Content, JsonSettings);

            if (response.StatusCode == HttpStatusCode.Forbidden)
            {
                return new PluginMisconfigurationException("Please sign up for an API subscription or check your access.");
            }

            if (error.Fields != null && error.Fields.Any())
            {
                var fieldsMessage = string.Join(", ", error.Fields.Values.Select(x => x.Message));
                return new PluginMisconfigurationException(fieldsMessage);
            }

            return new PluginApplicationException($"Error: {response.ErrorMessage}");
        }
        catch (Exception ex)
        {
            return new PluginApplicationException($"Error: {response.ErrorMessage}");

        }
    }

    public async Task<List<T>> Paginate<T>(RestRequest request)
    {
        var pageToken = "";
        //var limit = 200;

        var baseUrl = request.Resource; //.SetQueryParameter("maxReturn", limit.ToString());

        var result = new List<T>();
        BaseResponseDto<T> response;
        do
        {
            if(!string.IsNullOrEmpty(pageToken))
                request.Resource = baseUrl.SetQueryParameter("pageToken", pageToken);

            response = await ExecuteWithErrorHandling<BaseResponseDto<T>>(request);

            if (pageToken == response.NextPageToken)
                break;

            result.AddRange(response.Results ?? Enumerable.Empty<T>());
            pageToken = response.NextPageToken;
        } while (response.Results?.Any() is true);

        return result;
    }

}