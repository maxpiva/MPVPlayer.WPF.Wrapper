using System.Windows.Input;

namespace NutzCode.MPVPlayer.WPF.Wrapper.Models
{
    public class KeyBinding
    {
        public Key Key { get; set; }
        public KeyAction Action { get; set; }

        public KeyBinding(Key key, KeyAction action)
        {
            Key = key;
            Action = action;
        }
    }
}
