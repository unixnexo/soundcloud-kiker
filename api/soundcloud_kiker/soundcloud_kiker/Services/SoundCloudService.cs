using Microsoft.EntityFrameworkCore;
using soundcloud_kiker.Data;
using soundcloud_kiker.Models;
using soundcloud_kiker.Models.DTOs;
using System.Diagnostics;
using System.IO.Compression;
using System.Text;
using System.Text.Json;

namespace soundcloud_kiker.Services
{
    public class SoundCloudService
    {
        private readonly HttpClient _httpClient;
        //private readonly string _clientId = "FvkDBx5usilkOwhGPFsG97512Dhw7LRx";
        private readonly string _clientId;
        private readonly AppDbContext _db;

        public SoundCloudService(HttpClient httpClient, IConfiguration config, AppDbContext db)
        {
            _httpClient = httpClient;
            _clientId = config["SoundCloud:ClientId"];
            _db = db;
        }
        public async Task<List<PlaylistTrackDto>> GetPlaylistTracksAsync(string playlistUrl, bool onlyNew = false)
        {
            // Resolve the playlist
            var resolveUrl = $"https://api-v2.soundcloud.com/resolve?url={playlistUrl}&client_id={_clientId}";
            var response = await _httpClient.GetAsync(resolveUrl);
            response.EnsureSuccessStatusCode();

            var json = await response.Content.ReadAsStringAsync();
            using var doc = JsonDocument.Parse(json);
            var root = doc.RootElement;

            if (root.GetProperty("kind").GetString() != "playlist")
                throw new Exception("URL is not a playlist.");

            var playlistId = root.GetProperty("id").GetInt64().ToString();
            var tracks = new List<PlaylistTrackDto>();

            if (!root.TryGetProperty("tracks", out var collection) || collection.ValueKind != JsonValueKind.Array)
                return tracks;

            foreach (var track in collection.EnumerateArray())
            {
                var dto = new PlaylistTrackDto
                {
                    SoundCloudId = track.GetProperty("id").GetInt64().ToString(),
                    Title = track.GetProperty("title").GetString(),
                    Artist = track.GetProperty("user").GetProperty("username").GetString(),
                    Url = track.GetProperty("permalink_url").GetString(),
                    Thumbnail = track.TryGetProperty("artwork_url", out var art) ? art.GetString() : null,
                    StreamUrl = GetProgressiveStreamUrl(track)
                };

                tracks.Add(dto);
            }

            // Save or diff logic
            var playlist = await _db.Playlists.Include(p => p.Tracks).FirstOrDefaultAsync(p => p.SoundCloudId == playlistId);
            if (playlist == null)
            {
                playlist = new Playlist
                {
                    SoundCloudId = playlistId,
                    Url = playlistUrl,
                    Tracks = tracks.Select(t => new Track
                    {
                        SoundCloudId = t.SoundCloudId,
                        Title = t.Title,
                        Artist = t.Artist,
                        Url = t.Url,
                        Thumbnail = t.Thumbnail,
                        StreamUrl = t.StreamUrl
                    }).ToList()
                };

                _db.Playlists.Add(playlist);
                await _db.SaveChangesAsync();

                return tracks; // All are new anyway
            }

            if (onlyNew)
            {
                var existingIds = playlist.Tracks.Select(t => t.SoundCloudId).ToHashSet();
                var newTracks = tracks.Where(t => !existingIds.Contains(t.SoundCloudId)).ToList();

                // Save the new ones
                foreach (var t in newTracks)
                {
                    playlist.Tracks.Add(new Track
                    {
                        SoundCloudId = t.SoundCloudId,
                        Title = t.Title,
                        Artist = t.Artist,
                        Url = t.Url,
                        Thumbnail = t.Thumbnail,
                        StreamUrl = t.StreamUrl
                    });
                }

                if (newTracks.Any())
                    await _db.SaveChangesAsync();

                return newTracks;
            }
            else
            {
                // Resync: update any new tracks too
                var existingIds = playlist.Tracks.Select(t => t.SoundCloudId).ToHashSet();
                var newTracks = tracks.Where(t => !existingIds.Contains(t.SoundCloudId)).ToList();

                foreach (var t in newTracks)
                {
                    playlist.Tracks.Add(new Track
                    {
                        SoundCloudId = t.SoundCloudId,
                        Title = t.Title,
                        Artist = t.Artist,
                        Url = t.Url,
                        Thumbnail = t.Thumbnail,
                        StreamUrl = t.StreamUrl
                    });
                }

                if (newTracks.Any())
                    await _db.SaveChangesAsync();

                return tracks; // return all
            }
        }

        private string GetProgressiveStreamUrl(JsonElement track)
        {
            if (track.TryGetProperty("media", out var media) &&
                media.TryGetProperty("transcodings", out var transcodings) &&
                transcodings.ValueKind == JsonValueKind.Array)
            {
                foreach (var transcoding in transcodings.EnumerateArray())
                {
                    if (transcoding.GetProperty("format").GetProperty("protocol").GetString() == "progressive")
                    {
                        return $"{transcoding.GetProperty("url").GetString()}?client_id={_clientId}";
                    }
                }
            }
            return null;
        }

        public async Task<byte[]> DownloadTracksAsZipAsync(List<string> trackUrls)
        {
            var tempDir = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
            Directory.CreateDirectory(tempDir);

            var urlListPath = Path.Combine(tempDir, "urls.txt");
            await File.WriteAllLinesAsync(urlListPath, trackUrls);

            var startInfo = new ProcessStartInfo
            {
                FileName = "yt-dlp",
                Arguments = $"-x --audio-format mp3 -o \"{tempDir}/%(title)s.%(ext)s\" -a \"{urlListPath}\"",
                UseShellExecute = false,
                RedirectStandardOutput = false,
                RedirectStandardError = false,
                CreateNoWindow = true
            };

            using var process = Process.Start(startInfo);
            if (process != null)
                await process.WaitForExitAsync();

            var zipPath = Path.Combine(Path.GetTempPath(), $"{Guid.NewGuid()}.zip");
            ZipFile.CreateFromDirectory(tempDir, zipPath);

            var zipBytes = await File.ReadAllBytesAsync(zipPath);

            Directory.Delete(tempDir, true);
            File.Delete(zipPath);

            return zipBytes;
        }




    }
}
