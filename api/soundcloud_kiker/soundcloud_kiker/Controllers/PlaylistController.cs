using Microsoft.AspNetCore.Mvc;
using soundcloud_kiker.Data;
using soundcloud_kiker.Models.DTOs;
using soundcloud_kiker.Services;
using System.Diagnostics;
using System.IO.Compression;

namespace soundcloud_kiker.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class PlaylistController : Controller
    {
        private readonly AppDbContext _context;
        private readonly SoundCloudService _scService;

        public PlaylistController(AppDbContext context, SoundCloudService scService)
        {
            _context = context;
            _scService = scService;
        }

        [HttpGet]
        public async Task<IActionResult> GetPlaylist([FromQuery] string url)
        {
            var tracks = await _scService.GetPlaylistTracksAsync(url);

            var knownIds = _context.PlaylistTracks.Select(t => t.SoundCloudId).ToHashSet();

            var newTracks = tracks.Where(t => !knownIds.Contains(t.SoundCloudId)).ToList();

            return Ok(newTracks);
        }


        [HttpPost("download")]
        public async Task<IActionResult> DownloadPlaylist([FromBody] DownloadRequest request)
        {
            if (request.TrackUrls == null || !request.TrackUrls.Any())
                return BadRequest("Track URLs required.");

            var tempDir = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
            Directory.CreateDirectory(tempDir);

            foreach (var url in request.TrackUrls)
            {
                var startInfo = new ProcessStartInfo
                {
                    FileName = "yt-dlp",
                    Arguments = $"-x --audio-format mp3 -o \"{tempDir}/%(title)s.%(ext)s\" \"{url}\"",
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                    UseShellExecute = false,
                    CreateNoWindow = true
                };

                using var process = Process.Start(startInfo);
                if (process != null)
                {
                    await process.WaitForExitAsync();
                }
            }

            var zipPath = Path.Combine(Path.GetTempPath(), $"{Guid.NewGuid()}.zip");
            ZipFile.CreateFromDirectory(tempDir, zipPath);
            var zipBytes = await System.IO.File.ReadAllBytesAsync(zipPath);

            Directory.Delete(tempDir, true);
            System.IO.File.Delete(zipPath);

            return File(zipBytes, "application/zip", "playlist.zip");
        }

    }

}
