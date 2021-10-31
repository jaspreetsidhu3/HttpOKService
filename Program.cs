using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Serilog;
using Serilog.Events;
using System.Configuration;
using Microsoft.Extensions.Configuration;
using System.IO;
using Serilog.Sinks.Email;
using System.Net;

namespace HttpOKService
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var configuration = new ConfigurationBuilder()
            .SetBasePath(Directory.GetCurrentDirectory())
            .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true).Build();
            string filePath = configuration["localLogPath"];
            string fromEmailAddress = configuration["fromEmailAddress"];
            string fromEmailPassword = configuration["fromEmailPassword"];
            string toEmailAddress = configuration["toEmailAddress"];
            string emailSubject = configuration["emailSubject"];
            Log.Logger = new LoggerConfiguration()
                .MinimumLevel.Debug()
                .MinimumLevel.Override("Microsoft", Serilog.Events.LogEventLevel.Warning)
                .Enrich.FromLogContext()
                .WriteTo.RollingFile(filePath)
                .WriteTo.Email(new EmailConnectionInfo
                {
                    FromEmail = fromEmailAddress,
                    ToEmail = toEmailAddress,
                    MailServer = "smtp.gmail.com",
                    NetworkCredentials = new NetworkCredential
                    {
                        UserName = fromEmailAddress,
                        Password = fromEmailPassword
                    },
                    EnableSsl = true,
                    Port = 465,
                    EmailSubject = emailSubject
                },
                    outputTemplate: "{Timestamp:yyyy-MM-dd HH:mm:ss.fff zzz} [{Level}]: {Message}{NewLine}{Exception} " + "\nFrom HttpOKService",
                    batchPostingLimit: 10
                    , restrictedToMinimumLevel: Serilog.Events.LogEventLevel.Error)
                .CreateLogger();
            try
            {
                Log.Information("Starting up the service");
                CreateHostBuilder(args).Build().Run();
            }
            catch (Exception ex)
            {
                Log.Fatal(ex, "There was a problem while starting the service");
            }
            finally
            {
                Log.CloseAndFlush();
            }

        }
        public static IHostBuilder CreateHostBuilder(string[] args) =>
            Host.CreateDefaultBuilder(args)
            .UseWindowsService()
                .ConfigureServices((hostContext, services) =>
                {
                    services.AddHostedService<Worker>();
                })
                .UseSerilog();
    }
}
