namespace ImageSorter
{
    public class Root
    {
        public string title { get; set; }
        public string description { get; set; }
        public string imageViews { get; set; }
        public Time creationTime { get; set; }
        public Time modificationTime { get; set; }
        // public GeoData geoData { get; set; }
        // public GeoData geoDataExif { get; set; }
        public Time photoTakenTime { get; set; }
    }
}