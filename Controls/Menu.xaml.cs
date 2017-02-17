using System;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;

// ReSharper disable RedundantExtendsListEntry

namespace NutzCode.MPVPlayer.WPF.Wrapper.Controls
{
    public partial class Menu : UserControl
    {
        public static readonly DependencyProperty ConfigImageProperty = DependencyProperty.Register("ConfigImage", typeof(ImageSource), typeof(Menu), new FrameworkPropertyMetadata(new BitmapImage(new Uri(@"pack://application:,,,/NutzCode.MPVPlayer.WPF.Wrapper;component/images/config.png", UriKind.RelativeOrAbsolute))));
        public static readonly DependencyProperty ExitImageProperty = DependencyProperty.Register("ExitImage", typeof(ImageSource), typeof(Menu), new FrameworkPropertyMetadata(new BitmapImage(new Uri(@"pack://application:,,,/NutzCode.MPVPlayer.WPF.Wrapper;component/images/exit.png", UriKind.RelativeOrAbsolute))));
        public static readonly DependencyProperty AudioImageProperty = DependencyProperty.Register("AudioImage", typeof(ImageSource), typeof(Menu), new FrameworkPropertyMetadata(new BitmapImage(new Uri(@"pack://application:,,,/NutzCode.MPVPlayer.WPF.Wrapper;component/images/audio.png", UriKind.RelativeOrAbsolute))));
        public static readonly DependencyProperty SubtitleImageProperty = DependencyProperty.Register("SubtitleImage", typeof(ImageSource), typeof(Menu), new FrameworkPropertyMetadata(new BitmapImage(new Uri(@"pack://application:,,,/NutzCode.MPVPlayer.WPF.Wrapper;component/images/subs.png", UriKind.RelativeOrAbsolute))));
        public static readonly DependencyProperty MenuImageProperty = DependencyProperty.Register("MenuImage", typeof(ImageSource), typeof(Menu), new FrameworkPropertyMetadata(new BitmapImage(new Uri(@"pack://application:,,,/NutzCode.MPVPlayer.WPF.Wrapper;component/images/menu.png", UriKind.RelativeOrAbsolute))));
        public static readonly DependencyProperty MouseOverColorProperty = DependencyProperty.Register("MouseOverColor", typeof(Brush), typeof(Menu), new FrameworkPropertyMetadata(new SolidColorBrush(Color.FromArgb(0xFF, 0x00, 0x80, 0xd0))));
        public static readonly DependencyProperty BackgroundColorProperty = DependencyProperty.Register("BackgroundColor", typeof(Brush), typeof(Menu), new FrameworkPropertyMetadata(new SolidColorBrush(Color.FromArgb(0xFF, 0x30, 0x30, 0x30))));
        public static readonly DependencyProperty PaddingRelationProperty = DependencyProperty.Register("PaddingRelation", typeof(Size), typeof(Menu), new FrameworkPropertyMetadata(new Size(0.125, 0.125)));
        public static readonly DependencyProperty HasExitProperty = DependencyProperty.Register("HasExit", typeof(bool), typeof(Menu), new FrameworkPropertyMetadata(true));
        public static readonly DependencyProperty HasAudiosProperty = DependencyProperty.Register("HasAudios", typeof(bool), typeof(Menu), new FrameworkPropertyMetadata(true));
        public static readonly DependencyProperty HasSubsProperty = DependencyProperty.Register("HasSubs", typeof(bool), typeof(Menu), new FrameworkPropertyMetadata(true));

        private bool? _menuisshown;
        private Timer _timer;


        public Menu()
        {
            InitializeComponent();
            top.Visibility = Visibility.Collapsed;
            MouseLeave += Menu_MouseLeave;
            MouseEnter += Menu_MouseEnter;
            Open.Clicked += () => MenuIsShown = !MenuIsShown;
            Config.Clicked += () =>
            {
                FastOff();
                ConfigClicked?.Invoke();
            };
            Audio.Clicked += () =>
            {
                FastOff();
                AudioClicked?.Invoke();
            };
            Subs.Clicked += () =>
            {
                FastOff();
                SubtitlesClicked?.Invoke();
            };
            Exit.Clicked += () => ExitClicked?.Invoke();
        }

        public bool IsInside { get; private set; }

        public bool HasAudios
        {
            get { return (bool) GetValue(HasAudiosProperty); }
            set { SetValue(HasAudiosProperty, value); }
        }

        public bool HasSubs
        {
            get { return (bool) GetValue(HasSubsProperty); }
            set { SetValue(HasSubsProperty, value); }
        }

        public bool HasExit
        {
            get { return (bool) GetValue(HasExitProperty); }
            set { SetValue(HasExitProperty, value); }
        }

        public ImageSource ConfigImage
        {
            get { return (ImageSource) GetValue(ConfigImageProperty); }
            set { SetValue(ConfigImageProperty, value); }
        }

        public ImageSource AudioImage
        {
            get { return (ImageSource) GetValue(AudioImageProperty); }
            set { SetValue(AudioImageProperty, value); }
        }

        public ImageSource SubtitleImage
        {
            get { return (ImageSource) GetValue(SubtitleImageProperty); }
            set { SetValue(SubtitleImageProperty, value); }
        }

        public ImageSource MenuImage
        {
            get { return (ImageSource) GetValue(MenuImageProperty); }
            set { SetValue(MenuImageProperty, value); }
        }

        public ImageSource ExitImage
        {
            get { return (ImageSource) GetValue(ExitImageProperty); }
            set { SetValue(ExitImageProperty, value); }
        }

        public Size PaddingRelation
        {
            get { return (Size) GetValue(PaddingRelationProperty); }
            set { SetValue(PaddingRelationProperty, value); }
        }

        public Brush MouseOverColor
        {
            get { return (Brush) GetValue(MouseOverColorProperty); }
            set { SetValue(MouseOverColorProperty, value); }
        }

        public Brush BackgroundColor
        {
            get { return (Brush) GetValue(BackgroundColorProperty); }
            set { SetValue(BackgroundColorProperty, value); }
        }

        public TimeSpan ShowTimeout { get; set; } = TimeSpan.FromSeconds(3);

        public bool MenuIsShown
        {
            get
            {
                if (_menuisshown.HasValue)
                    return _menuisshown.Value;
                return top.Visibility == Visibility.Visible;
            }
            set
            {
                _menuisshown = value;
                top.VisbilityWithFade(value ? Visibility.Visible : Visibility.Collapsed);
            }
        }

        public event Delegates.ButtonChangeHandler ConfigClicked;
        public event Delegates.ButtonChangeHandler AudioClicked;
        public event Delegates.ButtonChangeHandler SubtitlesClicked;
        public event Delegates.ButtonChangeHandler ExitClicked;

        private void Menu_MouseEnter(object sender, MouseEventArgs e)
        {
            IsInside = true;
            _timer?.Dispose();
            _timer = null;
        }

        private void Menu_MouseLeave(object sender, MouseEventArgs e)
        {
            IsInside = false;
            if (MenuIsShown)
            {
                _timer?.Dispose();
                _timer = new Timer(Timeout, null, ShowTimeout, TimeSpan.FromMilliseconds(-1));
            }
        }

        private void Timeout(object obj)
        {
            _timer?.Dispose();
            _timer = null;
            Dispatcher.Invoke(() => { MenuIsShown = false; });
        }

        public void FastOff()
        {
            _menuisshown = false;
            top.Visibility = Visibility.Collapsed;
        }
    }
}