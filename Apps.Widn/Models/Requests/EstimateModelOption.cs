using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Apps.Widn.DataSources.Enums;
using Blackbird.Applications.Sdk.Common.Dictionaries;
using Blackbird.Applications.Sdk.Common;

namespace Apps.Widn.Models.Requests
{
    public class EstimateModelOption
    {
        [StaticDataSource(typeof(EstimateModelDataHandler))]
        [Display("Model")]
        public string Model { get; set; }
    }
}
