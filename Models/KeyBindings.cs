using System.Collections.Generic;
using System.Linq;
using System.Windows.Input;

namespace NutzCode.MPVPlayer.WPF.Wrapper.Models
{
    public class KeyBindings : List<KeyBinding>
    {
        public static KeyBindings Default = new KeyBindings()
        {
            new KeyBinding(Key.Space, KeyAction.PlayPauseToggle),
            new KeyBinding(Key.MediaPlayPause, KeyAction.PlayPauseToggle),
            new KeyBinding(Key.Play, KeyAction.Play),
            new KeyBinding(Key.Pause, KeyAction.Pause),
            new KeyBinding(Key.Left, KeyAction.Back15Seconds),
            new KeyBinding(Key.BrowserBack, KeyAction.Back15Seconds),
            new KeyBinding(Key.D5, KeyAction.Back15Seconds),
            new KeyBinding(Key.Right, KeyAction.Forward15Seconds),
            new KeyBinding(Key.BrowserForward,KeyAction.Forward15Seconds),
            new KeyBinding(Key.D6, KeyAction.Forward15Seconds),
            new KeyBinding(Key.D4, KeyAction.Back30Seconds),
            new KeyBinding(Key.D7, KeyAction.Forward30Seconds),
            new KeyBinding(Key.D3, KeyAction.Back1Minute),
            new KeyBinding(Key.D8, KeyAction.Forward1Minute),
            new KeyBinding(Key.D2, KeyAction.Back3Minutes),
            new KeyBinding(Key.D9, KeyAction.Forward3Minutes),
            new KeyBinding(Key.D1, KeyAction.Back5Minutes),
            new KeyBinding(Key.D0, KeyAction.Forward5Minutes),
            new KeyBinding(Key.VolumeDown, KeyAction.VolumeDown),
            new KeyBinding(Key.OemMinus, KeyAction.VolumeDown),
            new KeyBinding(Key.VolumeUp, KeyAction.VolumeUp),
            new KeyBinding(Key.OemPlus, KeyAction.VolumeUp),
            new KeyBinding(Key.VolumeMute, KeyAction.Mute),
            new KeyBinding(Key.Escape, KeyAction.WindowFullscreenToggle),
            new KeyBinding(Key.Enter, KeyAction.WindowFullscreenToggle),
            new KeyBinding(Key.F, KeyAction.Fullscreen),
            new KeyBinding(Key.Delete, KeyAction.Minimize),
            new KeyBinding(Key.S, KeyAction.SubtitleStreams),
            new KeyBinding(Key.A, KeyAction.AudioStreams),
            new KeyBinding(Key.C, KeyAction.Settings),
            new KeyBinding(Key.Q, KeyAction.Close),
            new KeyBinding(Key.X, KeyAction.Close),
            new KeyBinding(Key.NumPad0,KeyAction.Volume100Percent),
            new KeyBinding(Key.OemPeriod, KeyAction.Mute),
            new KeyBinding(Key.NumPad1, KeyAction.Volume10Percent),
            new KeyBinding(Key.NumPad2, KeyAction.Volume20Percent),
            new KeyBinding(Key.NumPad3, KeyAction.Volume30Percent),
            new KeyBinding(Key.NumPad4, KeyAction.Volume40Percent),
            new KeyBinding(Key.NumPad5, KeyAction.Volume50Percent),
            new KeyBinding(Key.NumPad6, KeyAction.Volume60Percent),
            new KeyBinding(Key.NumPad7, KeyAction.Volume70Percent),
            new KeyBinding(Key.NumPad8, KeyAction.Volume80Percent),
            new KeyBinding(Key.NumPad9, KeyAction.Volume90Percent)
        };

        private Dictionary<Key, KeyAction> _dict ;
        public KeyAction Translate(Key k)
        {
            if (_dict == null)
                _dict = this.ToDictionary(a => a.Key, a => a.Action);
            if (_dict.ContainsKey(k))
                return _dict[k];
            return KeyAction.None;
        }
    }
}
