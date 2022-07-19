using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System;
using System.Diagnostics;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Timers;
using Timer = System.Timers.Timer;

namespace StrimyDDNS
{
    internal class DynDnsService : IHostedService
    {
        private IOptionsMonitor<GlobalConfig> _config;
        private readonly ILogger<DynDnsService> _logger;
        private Timer _timer = new Timer();
        HttpClient client = new HttpClient();

        public DynDnsService(IOptionsMonitor<GlobalConfig> config, ILogger<DynDnsService> logger)
        {
            _config = config;
            _logger = logger;
            _logger.LogInformation("Starting DynDnsService");
        }

        public async Task StartAsync(CancellationToken cancellationToken)
        {
            await RunUpdate();

            _timer.Elapsed += _timer_Elapsed;
            _timer.Interval = _config.CurrentValue.Period * 1000 * 60;
            _timer.Start();
        }

        private async void _timer_Elapsed(object sender, ElapsedEventArgs e)
        {
            await RunUpdate();
        }

        public Task StopAsync(CancellationToken cancellationToken)
        {
            _timer.Stop();

            return Task.CompletedTask;
        }

        private async Task RunUpdate()
        {
            try
            {
                foreach (var item in _config.CurrentValue.Configs)
                {
                    await UpdateDns(item);
                }
            }
            catch(Exception ex)
            {
                _logger.LogError(ex, "Failed to run update");
            }
        }

        private async Task UpdateDns(DynDnsConfig dnsConfig)
        {
            int retry = 0;
            while (retry < 3)
            {     
                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Basic", Convert.ToBase64String(Encoding.Default.GetBytes($"{dnsConfig.User}:{dnsConfig.Password}")));
                //wc. = new NetworkCredential(dnsConfig.User, dnsConfig.Password);
                try
                {
                    var result = await client.GetStringAsync(dnsConfig.Url);
                    _logger.LogInformation($"Url : {dnsConfig.Url} - {result}");
                    break;
                }
                catch (Exception e)
                {
                    _logger.LogError(e, $"Failed on {dnsConfig.Url}");
                    retry++;
                    if (retry >= 3)
                        break;

                    await Task.Delay((int)(1000 * Math.Exp((double)retry)));
                }
            }
        }
    }
}