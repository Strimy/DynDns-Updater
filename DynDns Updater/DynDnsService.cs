using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Reflection;
using System.ServiceProcess;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Timers;
using Timer = System.Timers.Timer;

namespace DynDns_Updater
{
    public partial class DynDnsService : ServiceBase
    {
        private Config _config;
        private Timer _timer = new Timer();
        private EventLog eventLog;

        public DynDnsService()
        {
            InitializeComponent();

            eventLog = new System.Diagnostics.EventLog();
            if (!System.Diagnostics.EventLog.SourceExists("DynDnsUpdaterService"))
            {
                System.Diagnostics.EventLog.CreateEventSource(
                    "DynDns Updater Service", "Log");
            }
            eventLog.Source = "DynDns Updater Service";
            eventLog.Log = "Log";
        }

        protected override void OnStart(string[] args)
        {
            var directory = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
            using (StreamReader sr = new StreamReader(Path.Combine(directory, "config.json")))
            {
                using (JsonReader jr = new JsonTextReader(sr))
                {
                    _config = JsonSerializer.CreateDefault().Deserialize<Config>(jr);
                }
            }

            _timer.Interval = _config.Period * 1000.0 * 60.0;
            _timer.Elapsed += _timer_Elapsed;
            UpdateDns();
            _timer.Start();
        }

        private void _timer_Elapsed(object sender, ElapsedEventArgs e)
        {
            UpdateDns();
        }

        private void UpdateDns()
        {
            int retry = 0;
            while (retry < 3)
            {
                WebClient wc = new WebClient();
                wc.Credentials = new NetworkCredential(_config.User, _config.Password);
                try
                {
                    var result = wc.DownloadString(_config.Url);
                    eventLog.WriteEntry(result, EventLogEntryType.Information);
                    break;
                }
                catch (Exception e)
                {
                    retry++;
                    if (retry >= 3)
                        break;

                    eventLog.WriteEntry(e.ToString(), EventLogEntryType.Error);
                    Thread.Sleep((int)(1000 * Math.Exp((double)retry)));
                }
            }

        }

        protected override void OnStop()
        {
            _timer.Stop();
            _timer.Dispose();
        }
    }
}
