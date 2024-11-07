using Apps.Widn.Constants;
using Apps.Widn.Dtos;
using Apps.Widn.Invocables;
using Apps.Widn.Models.Requests;
using Apps.Widn.Models.Responses;
using Blackbird.Applications.Sdk.Common;
using Blackbird.Applications.Sdk.Common.Actions;
using Blackbird.Applications.Sdk.Common.Authentication;
using Blackbird.Applications.Sdk.Common.Invocation;
using Newtonsoft.Json;
using RestSharp;

namespace Apps.Widn.Actions;

[ActionList]
public class TranslationActions(InvocationContext invocationContext) : WidnInvocable(invocationContext)
{
    [Action("Translate text", Description = "Translate a text")]
    public async Task<TextTranslationResponse> TranslateText([ActionParameter] [Display("Source text")] string source, [ActionParameter] TranslateConfig config)
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
}