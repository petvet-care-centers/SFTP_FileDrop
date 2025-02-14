using Microsoft.Extensions.Hosting;
using System;
using System.Collections.Generic;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System;
using System.Threading;
using System.Threading.Tasks;
using SFTP_FileDrop_WorkerService.Classes;

namespace SFTP_FileDrop_WorkerService
{
    partial class Worker : BackgroundService
    {
        private readonly ILogger<Worker> _logger;
        private readonly CsvFileGeneration _csvFileGeneration;
        private bool _firstRun = true;

        public Worker(ILogger<Worker> logger, CsvFileGeneration csvFileGeneration)
        {
            _logger = logger;
            _csvFileGeneration = csvFileGeneration;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            try
            {
                while (!stoppingToken.IsCancellationRequested)
                {

                    if (_firstRun)
                    {
                        _logger.LogInformation("Service started manually - running now");
                        await ExecuteCsvGeneration();
                        _firstRun = false;

                    }

                    var now = DateTime.Now;
                    var scheduledTime = new DateTime(now.Year, now.Month, now.Day, 13, 0, 0);

                    if (now > scheduledTime)
                    {
                        scheduledTime = scheduledTime.AddDays(1);
                    }

                    var delay = scheduledTime - now;
                    _logger.LogInformation("Next job running at: {scheduledTime}");

                    await Task.Delay(delay, stoppingToken);

                    await ExecuteCsvGeneration();

                    await Task.Delay(TimeSpan.FromMinutes(1), stoppingToken);
                }

            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error has occurred while running the servivce.");
                throw;
            }
        }

        private async Task ExecuteCsvGeneration()
        {
            try
            {
                _logger.LogInformation("Starting CSV generation at: {time}", DateTimeOffset.Now);
                _csvFileGeneration.Run();
                _logger.LogInformation("CSV generation completed successfully");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during CSV generation");
            }
        }

        public override async Task StartAsync(CancellationToken cancellationToken)
        {
            _logger.LogInformation("Service is starting...");
            await base.StartAsync(cancellationToken);
        }

        public override async Task StopAsync(CancellationToken cancellationToken)
        {
            _logger.LogInformation("Service is stopping...");
            await base.StopAsync(cancellationToken);
        }
    }

}
