using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Text.RegularExpressions;
using MetadataExtractor.Formats.Avi;
using MetadataExtractor.Formats.Bmp;
using MetadataExtractor.Formats.Eps;
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
    public class Reader
    {
        public static bool TryReadMetadata(Stream stream, IDictionary<string, Directory> meta)
        {
            var fileType = FileTypeDetector.DetectFileType(stream);

            var metadata = (fileType switch
            {
                FileType.Arw => TiffMetadataReader.ReadMetadata(stream),
                FileType.Avi => AviMetadataReader.ReadMetadata(stream),
                FileType.Bmp => BmpMetadataReader.ReadMetadata(stream),
                FileType.Crx => QuickTimeMetadataReader.ReadMetadata(stream),
                FileType.Cr2 => TiffMetadataReader.ReadMetadata(stream),
                FileType.Eps => EpsMetadataReader.ReadMetadata(stream),
                FileType.Gif => GifMetadataReader.ReadMetadata(stream),
                FileType.Ico => IcoMetadataReader.ReadMetadata(stream),
                FileType.Jpeg => JpegMetadataReader.ReadMetadata(stream),
                FileType.Mp3 => Mp3MetadataReader.ReadMetadata(stream),
                FileType.Nef => TiffMetadataReader.ReadMetadata(stream),
                FileType.Netpbm => new Directory[] { NetpbmMetadataReader.ReadMetadata(stream) },
                FileType.Orf => TiffMetadataReader.ReadMetadata(stream),
                FileType.Pcx => new Directory[] { PcxMetadataReader.ReadMetadata(stream) },
                FileType.Png => PngMetadataReader.ReadMetadata(stream),
                FileType.Psd => PsdMetadataReader.ReadMetadata(stream),
                FileType.QuickTime => QuickTimeMetadataReader.ReadMetadata(stream),
                FileType.Mp4 => QuickTimeMetadataReader.ReadMetadata(stream),
                FileType.Raf => RafMetadataReader.ReadMetadata(stream),
                FileType.Rw2 => TiffMetadataReader.ReadMetadata(stream),
                FileType.Tga => TgaMetadataReader.ReadMetadata(stream),
                FileType.Tiff => TiffMetadataReader.ReadMetadata(stream),
                FileType.Wav => WavMetadataReader.ReadMetadata(stream),
                FileType.WebP => WebPMetadataReader.ReadMetadata(stream),
                FileType.Heif => HeifMetadataReader.ReadMetadata(stream),

                //FileType.Unknown   => throw new ImageProcessingException("File format could not be determined"),
                _ => Enumerable.Empty<Directory>()
            }).ToList();

            if (!metadata.Any())
            {
                return false;
            }
            foreach (var directory in metadata)
            {
                meta[directory.Name] = directory;
            }

            var fileTypeDirectory = new FileTypeDirectory(fileType);
            meta[fileTypeDirectory.Name] = fileTypeDirectory;
            return true;
        }
        public static bool TryReadMetadata(string filePath, IDictionary<string, Directory> metadata)
        {

            using (var stream = new FileStream(filePath, FileMode.Open, FileAccess.Read, FileShare.Read))
            {
                if (!TryReadMetadata(stream, metadata))
                {
                    return false;
                }
            }

            var m = Read(filePath);
            metadata[m.Name] = m;

            var s = ReadSidecar(filePath);
            metadata[s.Name] = s;
            return true;
        }
        public static SidecarMetadataDirectory ReadSidecar(string file)
        {
            var directory = new SidecarMetadataDirectory();

            if (File.Exists(file + ".json"))
            {
                var meta = JsonSerializer.Deserialize<Root>(File.ReadAllText(file + ".json"));

                directory.Set(SidecarMetadataDirectory.TagCreationTime, meta.creationTime.ToDateTime());
                directory.Set(SidecarMetadataDirectory.TagModificationTime, meta.modificationTime.ToDateTime());
                directory.Set(SidecarMetadataDirectory.TagPhotoTakenTime, meta.photoTakenTime.ToDateTime());
            }

            try
            {
                var name = Path.GetFileNameWithoutExtension(file);
                var result = Regex.Match(name, "((\\d{4}).?(\\d{2}).?(\\d{2})).?((\\d{2}).?(\\d{2}).?(\\d{2}))?");
                if (result.Success)
                {
                    if (result.Groups.Count >= 8)
                    {
                        directory.Set(SidecarMetadataDirectory.TagDateFromFilename,
                            new DateTime(
                                int.Parse(result.Groups[2].Value),
                                int.Parse(result.Groups[3].Value),
                                int.Parse(result.Groups[4].Value),
                                int.Parse(result.Groups[6].Value),
                                int.Parse(result.Groups[7].Value),
                                int.Parse(result.Groups[8].Value)
                            ));
                    }
                    else
                    {
                        directory.Set(SidecarMetadataDirectory.TagDateFromFilename,
                            new DateTime(
                                int.Parse(result.Groups[2].Value),
                                int.Parse(result.Groups[3].Value),
                                int.Parse(result.Groups[4].Value)
                            ));
                    }
                }
            }
            catch(Exception ex)
            {
                Console.WriteLine(ex);
            }

            return directory;
        }
        public static FileMetadataDirectory Read(string file)
        {
            var attr = File.GetAttributes(file);

            if ((attr & FileAttributes.Directory) != 0)
                throw new IOException("File object must reference a file");

            var fileInfo = new FileInfo(file);

            if (!fileInfo.Exists)
                throw new IOException("File does not exist");

            var directory = new FileMetadataDirectory();

            directory.Set(FileMetadataDirectory.TagFileName, Path.GetFileName(file));
            directory.Set(FileMetadataDirectory.TagFileSize, fileInfo.Length);
            directory.Set(FileMetadataDirectory.TagFileModifiedDate, fileInfo.LastWriteTime);

            return directory;
        }
    }
}
