using System.Collections.Generic;
using MetadataExtractor;

namespace ImageSorter
{
    public class SidecarMetadataDirectory : Directory
    {
        public const int TagCreationTime = 1;
        public const int TagModificationTime = 2;
        public const int TagPhotoTakenTime = 3;
        public const int TagDateFromFilename = 4;

        private static readonly Dictionary<int, string> _tagNameMap = new Dictionary<int, string>()
        {
            { TagCreationTime, "File Creation Time" },
            { TagModificationTime, "File Modification Time" },
            { TagPhotoTakenTime, "Photo Taken Time" },
            { TagDateFromFilename, "Date From Filename" },
        };

        public SidecarMetadataDirectory() : base(_tagNameMap)
        {
            SetDescriptor(new SidecarMetadataDescriptor(this));
        }

        public override string Name => "Sidecar";
    }
}