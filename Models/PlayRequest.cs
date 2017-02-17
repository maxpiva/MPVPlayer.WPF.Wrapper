using System.Collections.Generic;

namespace NutzCode.MPVPlayer.WPF.Wrapper.Models
{
    public class PlayRequest
    {
        public string Uri { get; set; }
        public List<string> ExternalSubtitles { get; set; }
        public List<string> ExternalAudios { get; set; }
        public bool IsPlaylist { get; set; }
        public long ResumePosition { get; set; }
        public string PreviewImageSourceUri { get; set; }
        public bool Autoplay { get; set; }
        public bool TakeScreenshotOnStart { get; set; }
        public double ScreenshotTimePercentage { get; set; } = 25;
    }
}