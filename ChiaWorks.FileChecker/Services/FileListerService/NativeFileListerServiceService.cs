using System.IO;

namespace ChiaWorks.FileChecker.Services.FileListerService
{
    public class NativeFileListerServiceService : IFileListerService
    {
        public string[] GetFileList(string path, string searchPattern, bool searchRecursive) =>
            Directory.GetFiles(path, searchPattern, searchRecursive ? SearchOption.AllDirectories : SearchOption.TopDirectoryOnly);

        public bool DirectoryExists(string path) => Directory.Exists(path);

        public string GetDeleteFileScript(string root, string file)
        {
            throw new System.NotImplementedException();
        }

        public string GetMoveFileScript(string root, string file, string target)
        {
            throw new System.NotImplementedException();
        }
    }
}