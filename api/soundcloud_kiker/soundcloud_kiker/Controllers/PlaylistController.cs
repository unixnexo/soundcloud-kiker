using Microsoft.AspNetCore.Mvc;
using soundcloud_kiker.Data;
using soundcloud_kiker.Services;

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


    }
}
