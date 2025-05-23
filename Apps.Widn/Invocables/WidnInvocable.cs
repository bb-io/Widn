using Apps.Widn.Api;
using Blackbird.Applications.Sdk.Common;
using Blackbird.Applications.Sdk.Common.Authentication;
using Blackbird.Applications.Sdk.Common.Invocation;

namespace Apps.Widn.Invocables;

public class WidnInvocable : BaseInvocable
{
    protected AuthenticationCredentialsProvider[] Creds =>
        InvocationContext.AuthenticationCredentialsProviders.ToArray();

    protected WidnClient Client { get; }
    public WidnInvocable(InvocationContext invocationContext) : base(invocationContext)
    {
        Client = new WidnClient(invocationContext.AuthenticationCredentialsProviders);
    }
}