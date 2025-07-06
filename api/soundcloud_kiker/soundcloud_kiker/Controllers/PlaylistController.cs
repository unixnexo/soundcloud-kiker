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
        private readonly DownloadService _downloadService;

        public PlaylistController(AppDbContext context, SoundCloudService scService, DownloadService downloadService)
        {
            _context = context;
            _scService = scService;
            _downloadService = downloadService;
        }

        [HttpGet("playlist")]
        public async Task<IActionResult> GetPlaylistTracks([FromQuery] string url, [FromQuery] bool onlyNew = false)
        {
            if (string.IsNullOrWhiteSpace(url))
                return BadRequest("Playlist URL is required.");

            var tracks = await _scService.GetPlaylistTracksAsync(url, onlyNew);
            return Ok(tracks);
        }


        [HttpPost("download")]
        public async Task<IActionResult> DownloadPlaylist([FromBody] DownloadRequest request)
        {
            if (request.TrackUrls == null || !request.TrackUrls.Any())
                return BadRequest("Track URLs required.");

            var zipBytes = await _scService.DownloadTracksAsZipAsync(request.TrackUrls);
            return File(zipBytes, "application/zip", "playlist.zip");
        }

    }

}
