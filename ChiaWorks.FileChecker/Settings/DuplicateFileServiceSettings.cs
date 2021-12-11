using System.Collections.Generic;

namespace ChiaWorks.FileChecker.Settings
{
    public class DuplicateFileServiceSettings
    {
        public List<string> SourcePaths { get; set; }
        public bool Recursive { get; set; }
        public string SearchPattern { get; set; } = "*.*";
    }
}