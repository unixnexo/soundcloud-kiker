using System.Diagnostics;
using System.IO.Compression;

namespace soundcloud_kiker.Services
{
    public class DownloadService
    {
        public async Task<byte[]> DownloadTracksAsZipAsync(List<string> trackUrls, string playlistName = "playlist")
        {
            if (trackUrls == null || !trackUrls.Any())
                throw new ArgumentException("Track URLs required.");

            var tempDir = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
            var urlListPath = Path.Combine(tempDir, "urls.txt");
            var zipPath = Path.Combine(Path.GetTempPath(), $"{Guid.NewGuid()}.zip");

            try
            {
                Directory.CreateDirectory(tempDir);

                await System.IO.File.WriteAllLinesAsync(urlListPath, trackUrls);

                var startInfo = new ProcessStartInfo
                {
                    FileName = "yt-dlp",
                    Arguments = $"-x --audio-format mp3 -o \"{tempDir}/%(title)s.%(ext)s\" -a \"{urlListPath}\"",
                    UseShellExecute = false,
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                    CreateNoWindow = true
                };

                using var process = Process.Start(startInfo);
                if (process == null)
                    throw new Exception("Failed to start yt-dlp process.");

                var output = await process.StandardOutput.ReadToEndAsync();
                var error = await process.StandardError.ReadToEndAsync();

                await process.WaitForExitAsync();

                if (process.ExitCode != 0)
                    throw new Exception($"yt-dlp failed.\nOutput: {output}\nError: {error}");

                ZipFile.CreateFromDirectory(tempDir, zipPath);
                var zipBytes = await System.IO.File.ReadAllBytesAsync(zipPath);

                return zipBytes;
            }
            finally
            {
                if (Directory.Exists(tempDir))
                    Directory.Delete(tempDir, true);

                if (System.IO.File.Exists(zipPath))
                    System.IO.File.Delete(zipPath);
            }
        }
    }
}
