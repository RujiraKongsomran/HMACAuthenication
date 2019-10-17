using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HMAC.Model
{
    class JSONGetLastETo
    {
        public class RootObject
        {
            public string date { get; set; }

            [JsonProperty("ETo[mm]")]
            public double eto { get; set; }
        }
    }
}
