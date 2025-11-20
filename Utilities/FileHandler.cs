using System;
using System.IO;

namespace EmployeeTimeTracker.Utilities
{
    public static class FileHandler
    {
        public static void AtomicWrite(string path, string data)
        {
            string temp = path + ".tmp";
            string backup = path + ".bak";

            // Write temp file first
            File.WriteAllText(temp, data);

            // Backup old version
            if (File.Exists(path))
            {
                File.Copy(path, backup, overwrite: true);
            }

            // Replace atomically
            File.Copy(temp, path, overwrite: true);

            // Cleanup
            File.Delete(temp);
        }

        public static string SafeRead(string path)
        {
            if (File.Exists(path))
            {
                return File.ReadAllText(path);
            }

            // If corrupted but backup exists → use backup
            string backup = path + ".bak";

            if (File.Exists(backup))
            {
                return File.ReadAllText(backup);
            }

            return "";
        }
    }
}
