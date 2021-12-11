using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;
using System.Timers;
using ChiaWorks.FileChecker.Extensions;
using ChiaWorks.FileChecker.Services;
using ChiaWorks.FileChecker.Services.FileListerService;
using ChiaWorks.FileChecker.Services.ScriptRunnerService;
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
        private static List<Action> _lateLogs = new List<Action>();

        public static void Main(string[] args)
        {
            RegisterDependencies();

            InitializeObjects();

            var awaiter = InnerRunApp().GetAwaiter();
            _lateLogs.ForEach(w => w.Invoke());
            if (_generalSettings.LoopCount == 1)
            {
                _logger.LogWarning("App will be closed after 1 cycle");

                awaiter.GetResult();
            }
            else
            {
                _logger.LogWarning("App will be running for ever");

                SpinWait.SpinUntil(() => false);
            }
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
                _logger.LogCritical(e.Message);
                _logger.LogCritical(e.ToJson());
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
            serviceProvider.AddScoped<DuplicateFileService>()
                .AddLogging(config =>
                    config.AddConsole(c => { c.TimestampFormat = "[dd:MM HH:mm:ss] "; })
                        .AddConfiguration(_configuration.GetSection("Logging")))
                ;

            RegisterOsDependantDependencies(serviceProvider);


            serviceProvider.Configure<DuplicateFileServiceSettings>(_configuration.GetSection(nameof(DuplicateFileServiceSettings)));
            serviceProvider.Configure<GeneralSettings>(_configuration.GetSection(nameof(GeneralSettings)));

            _services = serviceProvider.BuildServiceProvider();
        }

        private static void RegisterOsDependantDependencies(ServiceCollection serviceCollection)
        {
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux) || RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
            {
                serviceCollection.AddScoped<ScriptRunnerServiceBase, LinuxScriptRunnerService>();
            }

            var useRClone = _configuration.GetValue<bool>($"{nameof(GeneralSettings)}:{nameof(GeneralSettings.UseRClone)}");
            if (useRClone)
            {
                _lateLogs.Add(() => { _logger.LogInformation("rclone in use"); });
                serviceCollection.AddScoped<IFileListerService, RCloneFileListerServiceService>();
            }
            else
            {
                serviceCollection.AddScoped<IFileListerService, NativeFileListerServiceService>();
            }
        }
    }
}