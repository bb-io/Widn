using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Apps.Widn.Dtos
{
    public class Error
    {
        public int StatusCode { get; set; }
        public string Name { get; set; }
        public string Message { get; set; }
        public Dictionary<string, Field> Fields { get; set; }
    }

    public class Field
    {
        public string Message { get; set; }
    }
}
