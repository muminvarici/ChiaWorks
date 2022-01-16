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
        private readonly Dictionary<string, List<string>> _duplicateFilePaths;
        private readonly List<string> _duplicateRemovalFilePaths;
        private Dictionary<string, string[]> _rawFiles;
        private const string DuplicateFilesJsonPath = "duplicateFiles.json";

        public DuplicateFileService(IOptions<DuplicateFileServiceSettings> settings,
            ILogger<DuplicateFileService> logger,
            IFileListerService fileListerService)
        {
            _logger = logger;
            _fileListerService = fileListerService;
            _settings = settings.Value;

            _filePaths = new List<string>();
            _fileNames = new List<string>();
            _duplicateFilePaths = new Dictionary<string, List<string>>();
            _duplicateRemovalFilePaths = new List<string>();
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
            GenerateMoveString();
            if (!_duplicateFilePaths.IsNullOrEmpty())
            {
                _logger.LogDebug("Found duplicates. saving results...");
                await SaveResultAsync();
            }
            else
            {
                _logger.LogDebug("There is no duplicates deleting file");
                File.Delete(DuplicateFilesJsonPath);
            }
        }

        private void GenerateMoveString()
        {
            var moveFiles = new List<string>();
            var moveStrings = new List<string>();
            var deleteFiles = new List<string>();
            var deleteStrings = new List<string>();
            var fileNames = new List<string>();

            foreach (var root in _rawFiles)
            {
                root.Value.ToList().Sort(CustomSort);
                int folderCount = 0;
                int rootFolderCount = 1;
                foreach (var filePath in root.Value)
                {
                    var file = filePath.Replace("_(1)", " (1)").Replace("_(2)", " (2)");
                    var fileName = GetPureFileName(file);
                    if (fileNames.Contains(fileName))
                    {
                    }

                    fileNames.Add(fileName);
                    // if (file.Contains("party", StringComparison.CurrentCultureIgnoreCase))
                    // {
                    //     moveFiles.Add(fileName);
                    // }

                    if (!moveFiles.Contains(fileName))
                    {
                        if (!moveFiles.Contains(fileName))
                        {
                            moveFiles.Add(fileName);
                            if (!file.Contains("party", StringComparison.CurrentCultureIgnoreCase))
                            {
                                folderCount++;
                                if (folderCount == 3)
                                {
                                    rootFolderCount++;
                                    folderCount = 1;
                                }

                                if (rootFolderCount == 4)
                                {
                                    rootFolderCount = 1;
                                    folderCount = 1;
                                }

                                moveStrings.Add(_fileListerService.GetMoveFileScript("mumin", file, $"mumin:/Party{rootFolderCount}/plot{folderCount}/"));
                            }
                        }
                        else
                        {
                            deleteFiles.Add(fileName);
                            deleteStrings.Add(_fileListerService.GetDeleteFileScript("mumin", file));
                        }
                    }
                    else if (!deleteFiles.Contains(fileName))
                    {
                        deleteFiles.Add(fileName);
                        deleteStrings.Add(_fileListerService.GetDeleteFileScript("mumin", file));
                    }
                }
            }


            File.WriteAllText("moveStrings.json", moveStrings.ToJson()
                .Replace("[", "")
                .Replace("]", "")
                .Replace('"', ' ')
                .Replace(',', ' '));
            File.WriteAllText("deleteStrings.json", deleteStrings.ToJson()
                .Replace("[", "")
                .Replace("]", "")
                .Replace('"', ' ')
                .Replace(',', ' '));
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
            // _duplicateFilePaths.Sort(CustomSort);
            await File.WriteAllTextAsync(DuplicateFilesJsonPath, _duplicateFilePaths.ToJson());
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

                if (!duplicates.Any())
                    continue;

                foreach (var duplicate in duplicates)
                {
                    _logger.LogDebug($"Duplicate file {duplicate}");
                }

                var allDuplicates = files.Where(path => duplicates.Contains(GetPureFileName(path)))
                    .GroupBy(w => w.Substring(w.LastIndexOf(Path.DirectorySeparatorChar) < 0 ? 0 : w.LastIndexOf(Path.DirectorySeparatorChar) + 1))
                    .Where(w => w.Count() > 1)
                    .ToList();
                _logger.LogDebug($"There are {allDuplicates.Count} duplicate files");

                var allTriplicates = files.Where(w => duplicates.Contains(w.Substring(w.LastIndexOf(Path.DirectorySeparatorChar) < 0 ? 0 : w.LastIndexOf(Path.DirectorySeparatorChar) + 1)))
                    .GroupBy(w => w.Substring(w.LastIndexOf(Path.DirectorySeparatorChar) < 0 ? 0 : w.LastIndexOf(Path.DirectorySeparatorChar) + 1))
                    .Where(w => w.Count() > 2)
                    .ToList();
                _logger.LogDebug($"There are {allTriplicates.Count} triplicate or more files");

                var nonIdentical = files.Where(w => duplicates.Contains(w.Substring(w.LastIndexOf(Path.DirectorySeparatorChar) < 0 ? 0 : w.LastIndexOf(Path.DirectorySeparatorChar) + 1)))
                    .GroupBy(w => w.Substring(w.LastIndexOf(Path.DirectorySeparatorChar) < 0 ? 0 : w.LastIndexOf(Path.DirectorySeparatorChar) + 1))
                    .Select(x => x.Where(w => x.ToList().IndexOf(w) != 0).ToList())
                    .ToList();

                _duplicateRemovalFilePaths.AddRange(nonIdentical.SelectMany(w => w));

                foreach (var item in allDuplicates)
                {
                    while (!_duplicateFilePaths.TryAdd(item.Key, item.ToList()))
                    {
                        _logger.LogDebug("_duplicateFilePaths.TryAdd failed");
                    }
                }

                _fileNames.AddRange(innerFileNames);
                _filePaths.AddRange(files);
            }
        }

        private static string GetPureFileName(string path)
        {
            return path[(path.LastIndexOf(Path.DirectorySeparatorChar) < 0 ? 0 : path.LastIndexOf(Path.DirectorySeparatorChar) + 1)..];
        }
    }
}