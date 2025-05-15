using soundcloud_kiker.Models;
using System.Diagnostics;
using System.Text;
using System.Text.Json;

namespace soundcloud_kiker.Services
{
    public class SoundCloudService
    {
        public async Task<List<PlaylistTrack>> GetPlaylistTracksAsync(string playlistUrl)
        {
            var startInfo = new ProcessStartInfo
            {
                FileName = "yt-dlp",
                Arguments = $"-J \"{playlistUrl}\"",
                RedirectStandardOutput = true,
                UseShellExecute = false,
                CreateNoWindow = true
            };

            using var process = new Process { StartInfo = startInfo };
            process.Start();

            string output = await process.StandardOutput.ReadToEndAsync();
            await process.WaitForExitAsync();

            using var doc = JsonDocument.Parse(output);
            var root = doc.RootElement;

            var tracks = new List<PlaylistTrack>();

            if (!root.TryGetProperty("entries", out var entries))
                throw new Exception("Invalid playlist structure.");

            foreach (var entry in entries.EnumerateArray())
            {
                var id = entry.GetProperty("id").ToString();
                var title = entry.TryGetProperty("title", out var t) ? t.GetString() : "Unknown";
                var uploader = entry.TryGetProperty("uploader", out var u) ? u.GetString() : "Unknown";

                // Attempt to build a rough URL (yt-dlp doesn't return URL for playlists by default)
                var url = $"https://soundcloud.com/{uploader.ToLower().Replace(" ", "")}/{id}";

                // Get the largest thumbnail if available
                string thumbnail = null;
                if (entry.TryGetProperty("thumbnails", out var thumbs) && thumbs.GetArrayLength() > 0)
                {
                    var largest = thumbs[thumbs.GetArrayLength() - 1]; // usually highest res is last
                    thumbnail = largest.GetProperty("url").GetString();
                }

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
