using Apps.Widn.Dtos;
using Apps.Widn.Invocables;
using Blackbird.Applications.Sdk.Common.Dynamic;
using Blackbird.Applications.Sdk.Common.Invocation;
using RestSharp;

namespace Apps.Widn.DataSources
{
    public class LanguageDataHandler(InvocationContext invocationContext) : WidnInvocable(invocationContext), IAsyncDataSourceItemHandler
    {
        public async Task<IEnumerable<DataSourceItem>> GetDataAsync(DataSourceContext context, CancellationToken cancellationToken)
        {
            var request = new RestRequest("/language", Method.Get);
            var response = await Client.ExecuteWithErrorHandling<List<Language>>(request);
            return response.Where(x => context.SearchString == null || x.Name.Contains(context.SearchString, StringComparison.OrdinalIgnoreCase)).Select(x => new DataSourceItem(x.Locale, x.Name));
        }
    }
}
