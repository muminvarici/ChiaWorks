using System;
using System.Threading;
using ChiaWorks.FileChecker.Services;
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

            RunApp();
            SpinWait.SpinUntil(() => false);
        }

        private static void InitializeObjects()
        {
            _logger = _services.GetRequiredService<ILogger<Program>>();
            _logger.LogDebug("Objects initialized");
        }

        private static void RunApp()
        {
            try
            {

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
                .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true);

            _configuration = builder.Build();

            var serviceProvider = new ServiceCollection();
            serviceProvider.AddSingleton<DuplicateFileService>()
                .AddLogging(config=>config.AddConsole().AddConfiguration(_configuration.GetSection("Logging")))
                ;

            _services = serviceProvider.BuildServiceProvider();
        }
    }
}