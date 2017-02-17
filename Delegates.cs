using NutzCode.MPVPlayer.WPF.Wrapper.Models;

namespace NutzCode.MPVPlayer.WPF.Wrapper
{
    public class Delegates
    {
        public delegate void PlayStateChangeHandler(bool isplaying);

        public delegate void PositionChangeHandler(long position);

        public delegate void VolumeChangeHandler(double volume);

        public delegate void ButtonChangeHandler();

        public delegate void ChangePositionHandler(double delta);

        public delegate void SettingsChangedHandler(PlayerSettings pl);
    }
}
