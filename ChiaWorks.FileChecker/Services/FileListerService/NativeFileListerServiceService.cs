using System.IO;

namespace ChiaWorks.FileChecker.Services.FileListerService
{
    public class NativeFileListerServiceService : IFileListerService
    {
        public string[] GetFileList(string path, string searchPattern, bool searchRecursive) =>
            Directory.GetFiles(path, searchPattern, searchRecursive ? SearchOption.AllDirectories : SearchOption.TopDirectoryOnly);

        public bool DirectoryExists(string path) => Directory.Exists(path);
    }
}