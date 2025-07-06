namespace soundcloud_kiker.Models.DTOs
{
    public class PlaylistTrackDto
    {
        public string SoundCloudId { get; set; }
        public string Title { get; set; }
        public string Artist { get; set; }
        public string Url { get; set; }
        public string Thumbnail { get; set; }
        public string StreamUrl { get; set; }
    }
}
