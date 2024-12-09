using Apps.Widn.Constants;
using Apps.Widn.Dtos;
using Blackbird.Applications.Sdk.Common.Authentication;
using Blackbird.Applications.Sdk.Utils.Extensions.String;
using Blackbird.Applications.Sdk.Utils.RestSharp;
using Newtonsoft.Json;
using RestSharp;

namespace Apps.Widn.Api;

public class WidnClient : BlackBirdRestClient
{
    public WidnClient(IEnumerable<AuthenticationCredentialsProvider> authenticationCredentialsProviders) :
            base(new RestClientOptions { ThrowOnAnyError = false, BaseUrl = new Uri("https://api.widn.ai/v1") })
    {
        this.AddDefaultHeader("x-api-key", authenticationCredentialsProviders.First(x => x.KeyName == CredsNames.ApiKey).Value);
    }

    protected override Exception ConfigureErrorException(RestResponse response)
    {
        try
        {
            var json = response.Content!;
            var error = JsonConvert.DeserializeObject<Error>(json)!;

            if (error.Fields != null && error.Fields.Any())
            {
                return new(string.Join(", ", error.Fields.Values.Select(x => x.Message)));
            }

            return new($"{error.StatusCode} {error.Name} {error.Message}");
        }
        catch (Exception)
        {
            return new($"Failed to parse error response. Content: {response.Content}");
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