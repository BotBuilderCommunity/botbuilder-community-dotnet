using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Cortana_Assistant_Alexa_Sample.Models
{
    public class ExchangeRates
    {
        public class item
        {
            public string key { get; set; }

            public float value { get; set; }
        }

        public class RootObject
        {
            public bool success { get; set; }

            public string terms { get; set; }

            public string privacy { get; set; }

            public int timestamp { get; set; }

            public string source { get; set; }

            public Dictionary<string, double> quotes { get; set; }
        }
    }
}
