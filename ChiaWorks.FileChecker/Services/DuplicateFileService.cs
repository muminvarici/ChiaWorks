using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using ChiaWorks.FileChecker.Extensions;
using ChiaWorks.FileChecker.Services.FileListerService;
using ChiaWorks.FileChecker.Settings;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace ChiaWorks.FileChecker.Services
{
    public class DuplicateFileService
    {
        private readonly ILogger<DuplicateFileService> _logger;
        private readonly IFileListerService _fileListerService;
        private readonly DuplicateFileServiceSettings _settings;
        private readonly List<string> _filePaths;
        private readonly List<string> _fileNames;
        private readonly List<string> _duplicateFilePaths;
        private Dictionary<string, string[]> _rawFiles;
        private const string DuplicatefilesJsonPath = "duplicateFiles.json";


        public DuplicateFileService(IOptions<DuplicateFileServiceSettings> settings,
            ILogger<DuplicateFileService> logger,
            IFileListerService fileListerService)
        {
            _logger = logger;
            _fileListerService = fileListerService;
            _settings = settings.Value;

            _filePaths = new List<string>();
            _fileNames = new List<string>();
            _duplicateFilePaths = new List<string>();
        }

        public async Task RunAsync()
        {
            if (_settings.SourcePaths == null)
            {
                _logger.LogError("There is no path to check");
                return;
            }

            _logger.LogDebug("Getting file list");
            await GetFileListAsync();
            _logger.LogDebug("File list collected. finding duplicates...");
            FindDuplicates();
            if (!_duplicateFilePaths.IsNullOrEmpty())
            {
                _logger.LogDebug("Found duplicates. saving results...");
                await SaveResultAsync();
            }
            else
            {
                _logger.LogDebug("There is no duplicates deleting file");
                File.Delete(DuplicatefilesJsonPath);
            }
        }

        private async Task GetFileListAsync()
        {
            _rawFiles = new Dictionary<string, string[]>();
            var tasks = new List<Task>();
            foreach (var path in _settings.SourcePaths)
            {
                if (!_fileListerService.DirectoryExists(path))
                {
                    _logger.LogWarning($"Directory not exists: {path}");
                    continue;
                }

                tasks.Add(Task.Run(() =>
                {
                    _logger.LogDebug($"Working on path: {path}");
                    var files = _fileListerService.GetFileList(path, _settings.SearchPattern, _settings.Recursive);
                    if (files.Length == 0)
                    {
                        _logger.LogWarning($"Directory is empty: {path}");
                        return;
                    }

                    while (!_rawFiles.TryAdd(path, files))
                    {
                        _logger.LogWarning($"Directory & contents couldn't add to dictionary: {path}");
                        Thread.Sleep(500);
                    }

                    _logger.LogDebug($"Directory found {path} with {files.Length} files");
                }));
            }

            _logger.LogDebug("Waiting file collecting tasks to be completed...");
            await Task.WhenAll(tasks.ToArray());
            _logger.LogDebug("Tasks completed.");

        }

        private async Task SaveResultAsync()
        {
            _duplicateFilePaths.Sort(CustomSort);
            await File.WriteAllTextAsync(DuplicatefilesJsonPath, _duplicateFilePaths.ToJson());
        }

        private int CustomSort(string x, string y)
        {
            x = x.ReverseString();
            y = y.ReverseString();
            var comp = string.Compare(x, y, StringComparison.CurrentCultureIgnoreCase);
            return comp;
        }

        private void FindDuplicates()
        {
            foreach (var row in _rawFiles)
            {
                _logger.LogDebug($"Finding duplicates on path: {row.Key}");

                var files = row.Value;
                var innerFileNames = files.Select(Path.GetFileName).ToList();

                var duplicates = innerFileNames.Where(w => _fileNames.Contains(w))
                    .Concat(innerFileNames.GroupBy(x => x)
                        .Where(g => g.Count() > 1)
                        .Distinct()
                        .Select(y => y.Key))
                    .ToList();

                foreach (var duplicate in duplicates)
                {
                    _logger.LogWarning($"Duplicate file {duplicate}");
                }

                var allDuplicates = files.Where(w => duplicates.Contains(w.Substring(w.LastIndexOf(Path.DirectorySeparatorChar) < 0 ? 0 : w.LastIndexOf(Path.DirectorySeparatorChar) + 1))).ToList();
                _duplicateFilePaths.AddRange(allDuplicates);
                _fileNames.AddRange(innerFileNames);
                _filePaths.AddRange(files);
            }
        }
    }
}