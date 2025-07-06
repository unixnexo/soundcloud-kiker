namespace soundcloud_kiker.Models
{
    public class Track
    {
        public int Id { get; set; }
        public string? SoundCloudId { get; set; }
        public string? Title { get; set; }
        public string? Artist { get; set; }
        public string? Url { get; set; }
        public string? Thumbnail { get; set; }
        public string? StreamUrl { get; set; }

        public int PlaylistId { get; set; }
        public Playlist Playlist { get; set; }
    }
}
