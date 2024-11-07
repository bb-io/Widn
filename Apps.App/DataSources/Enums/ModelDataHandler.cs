using Blackbird.Applications.Sdk.Common.Dictionaries;
using Blackbird.Applications.Sdk.Common.Dynamic;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Apps.App.DataSources.Enums
{
    public class ModelDataHandler : IStaticDataSourceItemHandler
    {
        public IEnumerable<DataSourceItem> GetData() => new List<DataSourceItem>()
        {
            new("anthill", "Anthill"),
            new("sugarloaf", "Sugarloaf"),
            new("vesuvius", "Vesuvius"),
        };
    }
}
