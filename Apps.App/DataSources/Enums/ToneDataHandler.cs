using Blackbird.Applications.Sdk.Common.Dictionaries;
using Blackbird.Applications.Sdk.Common.Dynamic;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Apps.App.DataSources.Enums
{
    public class ToneDataHandler : IStaticDataSourceItemHandler
    {
        public IEnumerable<DataSourceItem> GetData() => new List<DataSourceItem>()
        {
            new("formal", "Formal"),
            new("informal", "Informal"),
            new("automatic", "Automatic"),
        };
    }
}
