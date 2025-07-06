namespace soundcloud_kiker.Models
{
    public class Playlist
    {
        public int Id { get; set; }
        public string? SoundCloudId { get; set; }
        public string? Url { get; set; }

        public List<Track> Tracks { get; set; } = new();
    }
}
