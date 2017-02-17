using System;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace NutzCode.MPVPlayer.WPF.Wrapper.Controls
{
    public class Play : Button
    {
        public static readonly DependencyProperty PlayImageProperty = DependencyProperty.Register("PlayImage", typeof(ImageSource), typeof(Play), new FrameworkPropertyMetadata(new BitmapImage(new Uri(@"pack://application:,,,/NutzCode.MPVPlayer.WPF.Wrapper;component/images/play.png", UriKind.RelativeOrAbsolute))));
        public static readonly DependencyProperty PauseImageProperty = DependencyProperty.Register("PauseImage", typeof(ImageSource), typeof(Play), new FrameworkPropertyMetadata(new BitmapImage(new Uri(@"pack://application:,,,/NutzCode.MPVPlayer.WPF.Wrapper;component/images/pause.png", UriKind.RelativeOrAbsolute))));

        private bool _isPlaying;


        public Play()
        {
            InitializeComponent();
            SetOpacityMask(false);
        }

        public bool IsPlaying
        {
            get { return _isPlaying; }
            set
            {
                if (value != _isPlaying)
                    SetOpacityMask(value);
                _isPlaying = value;
            }
        }


        public ImageSource PlayImage
        {
            get { return (ImageSource) GetValue(PlayImageProperty); }
            set
            {
                SetValue(PlayImageProperty, value);
                if (IsPlaying)
                    SetOpacityMask(IsPlaying);
            }
        }

        public ImageSource PauseImage
        {
            get { return (ImageSource) GetValue(PauseImageProperty); }
            set
            {
                SetValue(PauseImageProperty, value);
                if (!IsPlaying)
                    SetOpacityMask(IsPlaying);
            }
        }

        protected override void OnMouseUp(MouseButtonEventArgs e)
        {
            IsPlaying = !IsPlaying;
            PlayStateChanged?.Invoke(IsPlaying);
        }

        private void SetOpacityMask(bool value)
        {
            //Rectange do not support data triggers :(
            rect.OpacityMask = new ImageBrush(value ? PlayImage : PauseImage);
        }


        public event Delegates.PlayStateChangeHandler PlayStateChanged;
    }
}