using soundcloud_kiker.Models;
using System.Diagnostics;
using System.Text;
using System.Text.Json;

namespace soundcloud_kiker.Services
{
    public class SoundCloudService
    {
        private readonly HttpClient _httpClient;
        private readonly string _clientId = "FvkDBx5usilkOwhGPFsG97512Dhw7LRx";

        public SoundCloudService(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }

        public async Task<List<PlaylistTrack>> GetPlaylistTracksAsync(string playlistUrl)
        {
            var resolveUrl = $"https://api-v2.soundcloud.com/resolve?url={playlistUrl}&client_id={_clientId}";

            var response = await _httpClient.GetAsync(resolveUrl);
            response.EnsureSuccessStatusCode();

            var json = await response.Content.ReadAsStringAsync();
            using var doc = JsonDocument.Parse(json);

            var root = doc.RootElement;

            if (root.GetProperty("kind").GetString() != "playlist")
                throw new Exception("URL is not a playlist.");

            var tracks = new List<PlaylistTrack>();

            var collection = root.GetProperty("tracks");

            foreach (var track in collection.EnumerateArray())
            {
                string id = track.TryGetProperty("id", out var idProp) ? idProp.GetInt64().ToString() : "Unknown";

                string title = track.TryGetProperty("title", out var t) ? t.GetString() : "Unknown";

                string uploader = "Unknown";
                if (track.TryGetProperty("user", out var user) && user.TryGetProperty("username", out var username))
                {
                    uploader = username.GetString();
                }

                string url = track.TryGetProperty("permalink_url", out var w) ? w.GetString() : null;

                string thumbnail = track.TryGetProperty("artwork_url", out var art) ? art.GetString() : null;

                tracks.Add(new PlaylistTrack
                {
                    SoundCloudId = id,
                    Title = title,
                    Artist = uploader,
                    Url = url,
                    Thumbnail = thumbnail
                });
            }

            return tracks;
        }


    }
}
