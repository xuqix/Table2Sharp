using System.IO;
using System.Collections.Generic;

namespace Table2Sharp
{
    public static class Utils
    {
        public static string AbstractPath(string path)
        {
            return Path.IsPathRooted(path) ? path : Path.Combine(System.IO.Directory.GetCurrentDirectory(), path);
        }

        /// <summary>
        /// recursive get all file in dir
        /// </summary>
        /// <returns></returns>
        public static List<FileInfo> GetAllFiles(string dirPath, string searchPattern = "*")
        {
            List<FileInfo> files = new List<FileInfo>();

            DirectoryInfo dir = new DirectoryInfo(dirPath);
            if (!dir.Exists) return files;
            files.AddRange(dir.GetFiles(searchPattern));

            DirectoryInfo[] subDir = dir.GetDirectories();
            foreach (var d in subDir)
            {
                files.AddRange(GetAllFiles(dirPath + Path.DirectorySeparatorChar + d.ToString(), searchPattern));
            }
            return files;
        }
    }
}
