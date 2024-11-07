using Apps.App.Dtos;
using Apps.App.Invocables;
using Apps.App.Models.Requests;
using Apps.App.Models.Responses;
using Blackbird.Applications.Sdk.Common;
using Blackbird.Applications.Sdk.Common.Actions;
using Blackbird.Applications.Sdk.Common.Invocation;
using RestSharp;

namespace Apps.App.Actions;

[ActionList]
public class Actions(InvocationContext invocationContext) : WidnInvocable(invocationContext)
{
    [Action("Translate text", Description = "Translate a text")]
    public async Task<TextTranslationResponse> TranslateText([ActionParameter] [Display("Source text")] string source, [ActionParameter] TranslateConfig config)
    {
        var request = new RestRequest("/translate");
        request.AddJsonBody(new
        {
            sourceText = new List<string> { source },
            config,
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