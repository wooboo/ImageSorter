using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using MetadataExtractor;
using MetadataExtractor.Formats.Avi;
using MetadataExtractor.Formats.Bmp;
using MetadataExtractor.Formats.Eps;
using MetadataExtractor.Formats.Exif;
using MetadataExtractor.Formats.FileSystem;
using MetadataExtractor.Formats.FileType;
using MetadataExtractor.Formats.Gif;
using MetadataExtractor.Formats.Heif;
using MetadataExtractor.Formats.Ico;
using MetadataExtractor.Formats.Jpeg;
using MetadataExtractor.Formats.Mpeg;
using MetadataExtractor.Formats.Netpbm;
using MetadataExtractor.Formats.Pcx;
using MetadataExtractor.Formats.Photoshop;
using MetadataExtractor.Formats.Png;
using MetadataExtractor.Formats.QuickTime;
using MetadataExtractor.Formats.Raf;
using MetadataExtractor.Formats.Tga;
using MetadataExtractor.Formats.Tiff;
using MetadataExtractor.Formats.Wav;
using MetadataExtractor.Formats.WebP;
using MetadataExtractor.Util;
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
            var location = @"Z:\From L8mia";
            var files = System.IO.Directory.EnumerateFiles(location, "*.*", SearchOption.AllDirectories).ToList();

            Console.WriteLine($"File count: {files.Count()}");
            List<Task> tasks = new List<Task>();
            foreach (var file in files)
            {
                //tasks.Add(Task.Run(() =>
                //{
                try
                {
                    if (File.Exists(file))
                    {
                        var directories = new Dictionary<string, Directory>();
                        if (Reader.TryReadMetadata(file, directories))
                        {
                            //var dates = directories
                            //    .ToDictionary(o => o.Key,
                            //        o => o.Value.Tags.Where(t => t.Name.Contains("Date") || t.Name.Contains("Time")))
                            //    .Where(o => o.Value.Any())
                            //    .ToDictionary(o => o.Key,
                            //        o => o.Value.Select(t => new {t.Type, t.Name, t.Description}));
                            Console.WriteLine($"{c}/{files.Count}\t{file}");
                            //foreach (var date in dates)
                            //{
                            //    foreach (var values in date.Value)
                            //    {
                            //        Console.WriteLine($"{date.Key}\t{values.Name}\t{values.Type}\t{values.Description}");
                            //    }
                            //}

                            var d = GetDateTime(directories,
                                $"Sidecar:{SidecarMetadataDirectory.TagPhotoTakenTime}",
                                $"Sidecar:{SidecarMetadataDirectory.TagCreationTime}",
                                $"Exif IFD0:{ExifDirectoryBase.TagDateTime}",
                                $"Sidecar:{SidecarMetadataDirectory.TagDateFromFilename}",
                                $"File:{FileMetadataDirectory.TagFileModifiedDate}"
                            );
                            var path = Path.Combine("z:", d.ToString("yyyy-MM-dd"));
                            if (!System.IO.Directory.Exists(path))
                            {
                                System.IO.Directory.CreateDirectory(path);
                            }

                            var newPath = GetDuplicateFilePath(Path.Combine(path, Path.GetFileName(file)));
                            File.Move(file, newPath);
                            Console.WriteLine($"Moved to {newPath}");
                            if (File.Exists(file + ".json"))
                            {
                                var newJsonPath =
                                    GetDuplicateFilePath(Path.Combine(path, Path.GetFileName(file + ".json")));
                                File.Move(file + ".json", newJsonPath);
                            }

                            c++;
                        }

                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex);
                }

                //}));
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
            Console.ReadKey();
        }

        private static string GetDuplicateFilePath(string path)
        {
            var newPath = path;
            while (File.Exists(newPath))
            {

                var dir = Path.GetDirectoryName(path);
                var f = Path.GetFileNameWithoutExtension(path);
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
