using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace UniversalFileToPrinter.Helpers
{
    public class CastHelper
    {
        public static T CastTo<T>(object data)
        {
            string ss = JsonConvert.SerializeObject(data);
            return JsonConvert.DeserializeObject<T>(ss);
        }
    }
}
