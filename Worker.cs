using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

namespace HttpOKService
{
    public class Worker : BackgroundService
    {
        public IConfiguration Configuration { get; }
        private readonly ILogger<Worker> _logger;
        public HttpClient client;
        public Worker(ILogger<Worker> logger, IConfiguration configuration)
        {
            _logger = logger;
            Configuration = configuration;
        }
        public override Task StartAsync(CancellationToken cancellationToken)
        {
            client = new HttpClient();
            return base.StartAsync(cancellationToken);
        }
        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            try
            {
                var websiteLink = Configuration["websiteLink"];
                var result = await client.GetAsync(websiteLink);
                _logger.LogInformation($"Prefetch response from website {websiteLink} is successful: {result.IsSuccessStatusCode}");
                while (!stoppingToken.IsCancellationRequested)
                {
                    result = await client.GetAsync(websiteLink);
                    if (result.IsSuccessStatusCode)
                    {
                        _logger.LogInformation($"Your website {websiteLink} is UP with Status Code {result.StatusCode}");
                    }
                    else
                    {
                        _logger.LogError($"Your website {websiteLink} is down with Status Code {result.StatusCode}");
                    }
                    await Task.Delay(5000, stoppingToken);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError($"There was a problem while starting the service with exception message[" + ex.Message + "]");
            }
        }
        public override Task StopAsync(CancellationToken cancellationToken)
        {
            client.Dispose();
            _logger.LogInformation("The service has been stopped!");
            return base.StopAsync(cancellationToken);
        }
    }
}
