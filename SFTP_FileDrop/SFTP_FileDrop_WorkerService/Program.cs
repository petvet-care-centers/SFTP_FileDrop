using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System.IO;
using SFTP_FileDrop_WorkerService;
using SFTP_FileDrop_WorkerService.Classes;
using System;
using System.IO;


class Program
{
    static void Main(string[] args)
    {
        CreateHostBuilder(args).Build().Run();
        //var csvFileGeneration = host.Services.GetRequiredService<CsvFileGeneration>();
        //csvFileGeneration.Run();
    }

    public static IHostBuilder CreateHostBuilder(string[] args) =>
        Host.CreateDefaultBuilder(args)
        .UseWindowsService()
        .ConfigureAppConfiguration((context, config) =>
        {
            config.SetBasePath(Directory.GetCurrentDirectory());
            config.AddJsonFile("appSettings.json", optional: true, reloadOnChange: true);
        })
        .ConfigureServices((hostContext, services) =>
        {
            services.AddHostedService<Worker>();
            services.AddSingleton<CsvFileGeneration>();

            //Add other services

        });
    //Host.CreateDefaultBuilder(args)
    //    .ConfigureAppConfiguration((context, config) =>
    //    {
    //        config.SetBasePath(Directory.GetCurrentDirectory());
    //        config.AddJsonFile("appSettings.json", optional: true, reloadOnChange: true);
    //    })
    //    .ConfigureServices((context, services) =>
    //    {
    //        services.AddSingleton<CsvFileGeneration>();
    //    });
}