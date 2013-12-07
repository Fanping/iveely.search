using System.IO;

namespace NDatabase.IO
{
    internal static class OdbDirectory
    {
        internal static void Mkdirs(string filename)
        {
            var fullPath = Path.GetFullPath(filename);
            var directoryName = Path.GetDirectoryName(fullPath);

            if (directoryName == null)
                return;

            var directoryInfo = new DirectoryInfo(directoryName);

            if (!directoryInfo.Exists)
                directoryInfo.Create();
        }
    }
}
