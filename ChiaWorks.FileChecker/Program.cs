using System;
using System.Threading;
using System.Threading.Tasks;
using ChiaWorks.FileChecker.Services;
using ChiaWorks.FileChecker.Settings;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Configuration;

namespace ChiaWorks.FileChecker
{
    internal class Program
    {
        private static ServiceProvider _services;
        private static ILogger<Program> _logger;
        private static IConfigurationRoot _configuration;

        public static void Main(string[] args)
        {
            RegisterDependencies();

            InitializeObjects();

            RunApp().GetAwaiter();
            SpinWait.SpinUntil(() => false);
        }

        private static void InitializeObjects()
        {
            _logger = _services.GetRequiredService<ILogger<Program>>();
            _logger.LogDebug("Objects initialized");
        }

        private static async Task RunApp()
        {
            try
            {
                var duplicateFileService = _services.GetRequiredService<DuplicateFileService>();
                await duplicateFileService.RunAsync();
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                throw;
            }
        }

        private static void RegisterDependencies()
        {
            var builder = new ConfigurationBuilder()
                .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
                .AddJsonFile("appsettings.Debug.json", optional: true, reloadOnChange: true);
            ;

            _configuration = builder.Build();

            var serviceProvider = new ServiceCollection();
            serviceProvider.AddSingleton<DuplicateFileService>()
                .AddLogging(config => config.AddConsole().AddConfiguration(_configuration.GetSection("Logging")))
                ;

            serviceProvider.Configure<DuplicateFileServiceSettings>(_configuration.GetSection(nameof(DuplicateFileServiceSettings)));

            _services = serviceProvider.BuildServiceProvider();
        }
    }
}