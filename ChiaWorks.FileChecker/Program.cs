using System;
using System.Threading;
using System.Threading.Tasks;
using System.Timers;
using ChiaWorks.FileChecker.Extensions;
using ChiaWorks.FileChecker.Services;
using ChiaWorks.FileChecker.Settings;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Configuration;
using Microsoft.Extensions.Options;
using Timer = System.Timers.Timer;

namespace ChiaWorks.FileChecker
{
    internal class Program
    {
        private static ServiceProvider _services;
        private static ILogger<Program> _logger;
        private static IConfigurationRoot _configuration;
        private static Timer _timer;
        private static GeneralSettings _generalSettings;

        public static void Main(string[] args)
        {
            RegisterDependencies();

            InitializeObjects();

            InnerRunApp().GetAwaiter();
            SpinWait.SpinUntil(() => false);
        }

        private static void InitializeObjects()
        {
            _logger = _services.GetRequiredService<ILogger<Program>>();
            _logger.LogDebug("Objects initialized");

            _generalSettings = _services.GetRequiredService<IOptions<GeneralSettings>>().Value;
            _timer = new Timer
            {
                Interval = TimeSpan.FromSeconds(_generalSettings.Interval.NonZero(10 * 60)).TotalMilliseconds
            };
            _timer.Elapsed += (sender, args) => { InnerRunApp().GetAwaiter(); };
        }

        private static async Task InnerRunApp()
        {
            _timer.Stop();
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
            finally
            {
                _timer.Start();
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
            serviceProvider.Configure<GeneralSettings>(_configuration.GetSection(nameof(GeneralSettings)));

            _services = serviceProvider.BuildServiceProvider();
        }
    }
}