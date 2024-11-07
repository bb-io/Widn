using Apps.App.Constants;
using Apps.App.Dtos;
using Blackbird.Applications.Sdk.Common.Authentication;
using Blackbird.Applications.Sdk.Utils.RestSharp;
using Newtonsoft.Json;
using RestSharp;

namespace Apps.App.Api;

public class WidnClient : BlackBirdRestClient
{
    public WidnClient(IEnumerable<AuthenticationCredentialsProvider> authenticationCredentialsProviders) :
            base(new RestClientOptions { ThrowOnAnyError = false, BaseUrl = new Uri("https://api.staging.widn.ai/v1") }) // TODO: update from staging
    {
        this.AddDefaultHeader("X-Api-Key", authenticationCredentialsProviders.First(x => x.KeyName == CredsNames.ApiKey).Value);
    }

    protected override Exception ConfigureErrorException(RestResponse response)
    {
        try
        {
            var json = response.Content!;
            var error = JsonConvert.DeserializeObject<Error>(json)!;
            return new(error.Message);
        }
        catch (Exception)
        {
            return new($"Failed to parse error response. Content: {response.Content}");
        }
    }
}