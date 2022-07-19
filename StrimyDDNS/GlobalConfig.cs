using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StrimyDDNS
{
    public class GlobalConfig
    {
        public IEnumerable<DynDnsConfig> Configs { get; set; }
        public int Period { get; set; } = 120;

    }

    public class DynDnsConfig
    {
        public string Url { get; set; }
        public string User { get; set; }
        public string Password { get; set; }
    }
}
