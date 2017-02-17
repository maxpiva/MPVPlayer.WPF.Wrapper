using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

// ReSharper disable RedundantExtendsListEntry

namespace NutzCode.MPVPlayer.WPF.Wrapper.Controls
{
    public partial class Time : UserControl
    {
        private long _position;

        public Time()
        {
            InitializeComponent();
        }

        public long Position
        {
            get { return _position; }
            set
            {
                _position = value;
                SetPosition(value);
            }
        }

        public HorizontalAlignment Alignment
        {
            get { return time.HorizontalContentAlignment; }
            set { time.HorizontalContentAlignment = value; }
        }

        public event Delegates.ButtonChangeHandler Clicked;

        protected override void OnMouseUp(MouseButtonEventArgs e)
        {
            Clicked?.Invoke();
        }

        private void SetPosition(long value)
        {
            int sec = (int) (value % 60);
            int min = (int) (value / 60 % 60);
            int hour = (int) (value / 3600);
            time.Content = hour == 0 ? $"{min}:{sec:00}" : $"{hour}:{min:00}:{sec:00}";
        }
    }
}