using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using TMRazorImproved.Shared.Interfaces;

namespace TMRazorImproved.Core.Services
{
    public class VideoCaptureService : IVideoCaptureService
    {
        private readonly ILogger<VideoCaptureService> _logger;
        public bool IsRecording { get; private set; }

        public VideoCaptureService(ILogger<VideoCaptureService> logger)
        {
            _logger = logger;
        }

        public Task<bool> StartAsync(int fps = 15)
        {
            _logger.LogInformation("Video recording requested (Not yet implemented with SharpAvi)");
            return Task.FromResult(false);
        }

        public Task StopAsync()
        {
            IsRecording = false;
            return Task.CompletedTask;
        }
    }
}
