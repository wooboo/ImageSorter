using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using MetadataExtractor;
using MetadataExtractor.Formats.Exif;
using MetadataExtractor.Formats.FileSystem;
using Directory = MetadataExtractor.Directory;

namespace ImageSorter
{
    class Program
    {
        static async Task Main(string[] args)
        {
            var timer = new Stopwatch();
            timer.Start();
            int c = 0;
            var location = args[0];
            var files = System.IO.Directory.EnumerateFiles(location, "*.*", SearchOption.AllDirectories).ToList();

            Console.WriteLine($"File count: {files.Count()}");
            List<Task> tasks = new List<Task>();
            foreach (var file in files)
            {

                try
                {
                    if (File.Exists(file))
                    {
                        Console.WriteLine($"{c}/{files.Count}\t{file}");
                        var directories = new Dictionary<string, Directory>();
                        if (Reader.TryReadMetadata(file, directories))
                        {

                            var d = GetDateTime(directories,
                                $"Sidecar:{SidecarMetadataDirectory.TagPhotoTakenTime}",
                                $"Sidecar:{SidecarMetadataDirectory.TagCreationTime}",
                                $"Exif SubIFD:{ExifSubIfdDirectory.TagDateTimeOriginal}",
                                $"Exif IFD0:{ExifDirectoryBase.TagDateTime}",
                                $"Sidecar:{SidecarMetadataDirectory.TagDateFromFilename}",
                                $"File:{FileMetadataDirectory.TagFileModifiedDate}"
                            );
                            var newPath = MoveFile(args, file, d);

                            Console.WriteLine($" moved to {newPath}");
                        }
                        else
                        {
                            Console.WriteLine($" ignored");
                        }
                    }
                    else
                    {
                        Console.WriteLine($" not found");
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex);
                }

                c++;
            }

            if (!System.IO.Directory.EnumerateFileSystemEntries(location).Any())
            {
                System.IO.Directory.Delete(location);
            }
            //await Task.WhenAll(tasks);
            timer.Stop();

            TimeSpan timeTaken = timer.Elapsed;
            Console.WriteLine();
            Console.WriteLine($"{c} taken: {timeTaken}");
        }
        private static string MoveFile(string[] args, string file, DateTime d)
        {
            var path = Path.Combine(args[1], d.ToString("yyyy-MM-dd"));
            if (!System.IO.Directory.Exists(path))
            {
                System.IO.Directory.CreateDirectory(path);
            }

            var newPath = GetDuplicateFilePath(Path.Combine(path, Path.GetFileName(file)));
            File.Move(file, newPath);
            if (File.Exists(file + ".json"))
            {
                var newJsonPath =
                    GetDuplicateFilePath(Path.Combine(path, Path.GetFileName(file + ".json")));
                File.Move(file + ".json", newJsonPath);
            }
            return newPath;
        }
        private static string GetDuplicateFilePath(string path)
        {
            var newPath = path;
            while (File.Exists(newPath))
            {

                var dir = Path.GetDirectoryName(path);
                var f = Path.GetFileNameWithoutExtension(newPath);
                var ext = Path.GetExtension(path);
                newPath = Path.Combine(dir, f + "d" + ext);
            }

            return newPath;
        }
        public static DateTime GetDateTime(Dictionary<string, Directory> directories, params string[] fields)
        {

            foreach (var field in fields)
            {
                var f = field.Split(':');
                if (TryGetDateTime(directories, f[0], int.Parse(f[1]), out var result))
                    return result;
            }

            return default;
        }

        public static bool TryGetDateTime(Dictionary<string, Directory> directories, string name, int type, out DateTime dateTime)
        {
            if (directories.TryGetValue(name, out var directory))
            {
                if (directory.TryGetDateTime(type, out dateTime))
                    return true;
            }

            dateTime = default;
            return false;
        }

        public class Selector
        {
            public Selector()
            {

            }
        }
    }
}
