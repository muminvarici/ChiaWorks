using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using ChiaWorks.FileChecker.Extensions;
using ChiaWorks.FileChecker.Services.ScriptRunnerService;
using ChiaWorks.FileChecker.Settings;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace ChiaWorks.FileChecker.Services.FileListerService
{
    public class RCloneFileListerServiceService : IFileListerService
    {
        private readonly ScriptRunnerServiceBase _scriptRunner;
        private readonly ILogger<RCloneFileListerServiceService> _logger;
        private readonly GeneralSettings _generalSettings;

        public RCloneFileListerServiceService(ScriptRunnerServiceBase scriptRunner,
            ILogger<RCloneFileListerServiceService> logger,
            IOptions<GeneralSettings> generalSettings)
        {
            _scriptRunner = scriptRunner;
            _logger = logger;
            _generalSettings = generalSettings.Value;
        }

        public string[] GetFileList(string path, string searchPattern, bool searchRecursive)
        {
            var commandResult = GetCommandResult(path, searchRecursive);
            var result = ParseListResult(commandResult);
            searchPattern ??= "*.*";

            return result.ToArray();
            // return result.Where(w => Regex.IsMatch(w, searchPattern)).ToArray(); TODO implement search pattern
        }

        private string GetCommandResult(string path, bool searchRecursive)
        {
            string maxDeph = string.Empty;
            if (!searchRecursive)
            {
                maxDeph = "--max-depth 1";
            }

            string commandResult;
            if (_generalSettings.UseMock)
            {
                string fileName = $"{path.Split(':')[0]}_rcloneLsResult.txt";
                if (File.Exists(fileName))
                {
                    commandResult = File.ReadAllText(fileName);
                }
                else
                {
                    commandResult = _scriptRunner.RunCommand(RCloneCommands.ListFiles.Format(path, maxDeph));
                    File.WriteAllText(fileName, commandResult);
                }
            }
            else
            {
                _logger.LogDebug("RClone will list files, that may take much time");
                commandResult = _scriptRunner.RunCommand(RCloneCommands.ListFiles.Format(path, maxDeph));
            }

            return commandResult;
        }

        private IEnumerable<string> ParseListResult(string result)
        {
            if (result.IsNullOrWhiteSpace() || result.Contains("error", StringComparison.InvariantCultureIgnoreCase))
            {
                return null;
            }

            var rows = result.Trim().Split("\n");
            return rows.Select(w =>
            {
                var rowItems = w.Split(" ");
                if (rowItems.Length == 2)
                {
                    return rowItems[1];
                }

                return "";
            });
        }

        public bool DirectoryExists(string path)
        {
            _logger.LogDebug("Rclone DirectoryExists returned true");
            //todo implement in best way
            return true;
        }
    }
}