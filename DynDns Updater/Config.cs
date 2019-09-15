using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DynDns_Updater
{
    public class Config
    {
        public string Url { get; set; }
        public string User { get; set; }
        public string Password { get; set; }
        public int Period { get; set; } = 120;
    }
}
