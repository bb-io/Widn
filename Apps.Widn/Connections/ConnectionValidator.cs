using Apps.Widn.Api;
using Apps.Widn.Dtos;
using Apps.Widn.Models.Requests;
using Blackbird.Applications.Sdk.Common.Authentication;
using Blackbird.Applications.Sdk.Common.Connections;
using RestSharp;

namespace Apps.Widn.Connections;

public class ConnectionValidator: IConnectionValidator
{
    public async ValueTask<ConnectionValidationResponse> ValidateConnection(
        IEnumerable<AuthenticationCredentialsProvider> authenticationCredentialsProviders,
        CancellationToken cancellationToken)
    {
        var client = new WidnClient(authenticationCredentialsProviders);
        var request = new RestRequest("/translate", Method.Post);
        request.AddJsonBody(new
        {
            config = new TranslateConfig { SourceLocale = "es", TargetLocale = "en" },
            sourceText = new List<string> { "Hola" },
        });

        try
        {
            var response = await client.ExecuteWithErrorHandling(request);
        } catch (Exception ex) {
            return new()
            {
                IsValid = false,
                Message = ex.Message,
            };
        }
        

        return new()
        {
            IsValid = true
        };
    }
}