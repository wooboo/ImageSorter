using MetadataExtractor;

namespace ImageSorter
{
    public class SidecarMetadataDescriptor : TagDescriptor<SidecarMetadataDirectory>
    {
        public SidecarMetadataDescriptor(SidecarMetadataDirectory directory)
            : base(directory)
        {
        }
    }
}