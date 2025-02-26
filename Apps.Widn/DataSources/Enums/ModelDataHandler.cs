using Blackbird.Applications.Sdk.Common.Dictionaries;
using Blackbird.Applications.Sdk.Common.Dynamic;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Apps.Widn.DataSources.Enums
{
    public class ModelDataHandler : IStaticDataSourceItemHandler
    {
        public IEnumerable<DataSourceItem> GetData() => new List<DataSourceItem>()
        {
            new("anthill", "Anthill (fastest)"),
            new("sugarloaf", "Sugarloaf (balanced)"),
            new("vesuvius", "Vesuvius (highest quality)"),
        };
    }
}
