using System.ComponentModel.DataAnnotations;

namespace soundcloud_kiker.Models
{
    public class PlaylistTrack
    {
        [Key]
        public string SoundCloudId { get; set; }
        public string Title { get; set; }
        public string Artist { get; set; }
        public string Url { get; set; }
        public string Thumbnail { get; set; }
    }
}
