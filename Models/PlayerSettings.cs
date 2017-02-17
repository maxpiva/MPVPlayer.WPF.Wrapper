namespace NutzCode.MPVPlayer.WPF.Wrapper.Models
{
    public class PlayerSettings
    {
        public bool HardwareDecoding { get; set; } = true;
        public string AudioOutput { get; set; } = "auto";
        public string Channels { get; set; } = "auto";
        public string Device { get; set; } = "auto";
        public string Passthrough { get; set; } = "";
        public bool ExclusiveMode { get; set; } = false;
        public bool NormalizeVolume { get; set; } = false;
        public bool SubtitlesEnabled { get; set; } = true;
        public string PreferredAudios { get; set; } = string.Empty;
        public string PreferredSubtitles { get; set; } = string.Empty;
        public double SubtitleSize { get; set; } = 1.0;
        public KeyBindings KeyBindings { get; set; } = null;
    }
}
