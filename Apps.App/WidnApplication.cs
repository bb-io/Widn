using Blackbird.Applications.Sdk.Common;
using Blackbird.Applications.Sdk.Common.Metadata;

namespace Apps.App;

public class WidnApplication : IApplication, ICategoryProvider
{
    public IEnumerable<ApplicationCategory> Categories
    {
        get => [ApplicationCategory.ArtificialIntelligence, ApplicationCategory.MachineTranslationAndMtqe];
        set { }
    }

    public T GetInstance<T>()
    {
        throw new NotImplementedException();
    }
}