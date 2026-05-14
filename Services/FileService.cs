using System.Diagnostics;

namespace InventurApp.Services
{
    public class FileService
    {
        public List<FileInfo> GetFiles(string path)
        {
            if (string.IsNullOrWhiteSpace(path))
                return new List<FileInfo>();

            if (!Directory.Exists(path))
                Directory.CreateDirectory(path);

            var dir = new DirectoryInfo(path);
            return dir.GetFiles()
                .OrderByDescending(f => f.LastWriteTime)
                .ToList();
        }

        public void OpenFile(string path)
        {
            if (!File.Exists(path))
                throw new FileNotFoundException("Die Datei wurde nicht gefunden.", path);

            Process.Start(new ProcessStartInfo
            {
                FileName = path,
                UseShellExecute = true
            });
        }

        public void DeleteFile(string path)
        {
            if (!File.Exists(path))
                return;

            File.Delete(path);
        }
    }
}
