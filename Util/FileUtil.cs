using System.IO;

namespace RDEditorPlus.Util
{
    public static class FileUtil
    {
        private static string assemblyDirectory = null;
        public static string AssemblyDirectory => assemblyDirectory ??= Path.GetDirectoryName(typeof(FileUtil).Assembly.Location);

        public static string GetFilePathFromAssembly(string path)
        {
            return Path.Combine(AssemblyDirectory, path);
        }
    }
}
