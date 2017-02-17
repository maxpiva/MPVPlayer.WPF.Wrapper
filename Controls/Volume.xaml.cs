using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using NutzCode.MPVPlayer.WPF.Wrapper.Annotations;
// ReSharper disable VirtualMemberCallInConstructor
// ReSharper disable CompareOfFloatsByEqualityOperator

// ReSharper disable RedundantExtendsListEntry

namespace NutzCode.MPVPlayer.WPF.Wrapper.Controls
{
    public partial class Volume : UserControl, INotifyPropertyChanged
    {
        public static readonly DependencyProperty VolumeImageProperty = DependencyProperty.Register("VolumeImage", typeof(ImageSource), typeof(Volume), new FrameworkPropertyMetadata(new BitmapImage(new Uri(@"pack://application:,,,/NutzCode.MPVPlayer.WPF.Wrapper;component/images/volume.png", UriKind.RelativeOrAbsolute))));
        public static readonly DependencyProperty MuteImageProperty = DependencyProperty.Register("MuteImage", typeof(ImageSource), typeof(Volume), new FrameworkPropertyMetadata(new BitmapImage(new Uri(@"pack://application:,,,/NutzCode.MPVPlayer.WPF.Wrapper;component/images/mute.png", UriKind.RelativeOrAbsolute))));
        public static readonly DependencyProperty MouseOverColorProperty = DependencyProperty.Register("MouseOverColor", typeof(Brush), typeof(Volume), new FrameworkPropertyMetadata(new SolidColorBrush(Color.FromArgb(0xFF, 0x00, 0x80, 0xd0))));
        public static readonly DependencyProperty PaddingRelationProperty = DependencyProperty.Register("PaddingRelation", typeof(Size), typeof(Volume), new FrameworkPropertyMetadata(new Size(0.125D, 0.125D)));
        public static readonly DependencyProperty KnobImageProperty = DependencyProperty.Register("KnobImage", typeof(ImageSource), typeof(Volume), new FrameworkPropertyMetadata(new BitmapImage(new Uri(@"pack://application:,,,/NutzCode.MPVPlayer.WPF.Wrapper;component/images/knob.png", UriKind.RelativeOrAbsolute))));
        public static readonly DependencyProperty BottomBarColorProperty = DependencyProperty.Register("BottomBarColor", typeof(Brush), typeof(Volume), new FrameworkPropertyMetadata(new SolidColorBrush(Color.FromArgb(0xFF, 0x00, 0x80, 0xd0))));
        public static readonly DependencyProperty TopBarColorProperty = DependencyProperty.Register("TopBarColor", typeof(Brush), typeof(Volume), new FrameworkPropertyMetadata(new SolidColorBrush(Color.FromArgb(0xFF, 0x00, 0x00, 0x00))));
        public static readonly DependencyProperty BackgroundColorProperty = DependencyProperty.Register("BackgroundColor", typeof(Brush), typeof(Volume), new FrameworkPropertyMetadata(new SolidColorBrush(Color.FromArgb(0xFF, 0x30, 0x30, 0x30))));
        public static readonly DependencyProperty BarSizeProperty = DependencyProperty.Register("BarSize", typeof(double), typeof(Volume), new FrameworkPropertyMetadata(10.0D));
        public static readonly DependencyProperty KnobSizeProperty = DependencyProperty.Register("KnobSize", typeof(double), typeof(Volume), new FrameworkPropertyMetadata(20.0D));

        private bool? _sliderIsShown;
        private Timer _timer;

        private double _volume;

        public Volume()
        {
            InitializeComponent();
            sbar.Minimum = 0;
            sbar.Maximum = 1.0;
            sbar.ValueChanged += Sbar_ValueChanged;
            back.Visibility = Visibility.Collapsed;
            MouseLeave += Volume_MouseLeave;
            MouseEnter += Volume_MouseEnter;
            but.MouseDown += But_Click;
            SizeChanged += Volume_SizeChanged;
            _volume = .5;
            sbar.SmallChange = .1;
            sbar.LargeChange = .1;
            OnPropertyChanged("BarSize");
            OnPropertyChanged("HalfBarSize");
            OnPropertyChanged("DoubleBarSize");

            OnPropertyChanged("KnobSize");
            OnPropertyChanged("HalfKnobSize");
            OnPropertyChanged("BottomBarColor");
            OnPropertyChanged("TopBarColor");
            OnPropertyChanged("KnobImage");
            OnPropertyChanged("VolumeLevel");
        }

        public bool IsInside { get; private set; }

        public Size PaddingRelation
        {
            get { return (Size) GetValue(PaddingRelationProperty); }
            set
            {
                SetValue(PaddingRelationProperty, value);
                OnPropertyChanged("TopPadding");
            }
        }

        public TimeSpan ShowTimeout { get; set; } = TimeSpan.FromSeconds(3);

        public double TopPadding => ActualWidth * PaddingRelation.Height;

        public bool SliderIsShown
        {
            get
            {
                if (_sliderIsShown.HasValue)
                    return _sliderIsShown.Value;
                return back.Visibility == Visibility.Visible;
            }
            set
            {
                _sliderIsShown = value;
                List<UIElement> elements = new List<UIElement> {back};
                elements.VisbilityMembersWithFade(value ? Visibility.Visible : Visibility.Collapsed);
            }
        }


        public Brush BottomBarColor
        {
            get { return (Brush) GetValue(BottomBarColorProperty); }
            set { SetValue(BottomBarColorProperty, value); }
        }

        public Brush BackgroundColor
        {
            get { return (Brush) GetValue(BackgroundColorProperty); }
            set { SetValue(BackgroundColorProperty, value); }
        }

        public Brush TopBarColor
        {
            get { return (Brush) GetValue(TopBarColorProperty); }
            set { SetValue(TopBarColorProperty, value); }
        }

        public Brush MouseOverColor
        {
            get { return (Brush) GetValue(MouseOverColorProperty); }
            set { SetValue(MouseOverColorProperty, value); }
        }

        public ImageSource KnobImage
        {
            get { return (ImageSource) GetValue(KnobImageProperty); }
            set { SetValue(KnobImageProperty, value); }
        }

        public ImageSource MuteImage
        {
            get { return (ImageSource) GetValue(MuteImageProperty); }
            set { SetValue(MuteImageProperty, value); }
        }

        public ImageSource VolumeImage
        {
            get { return (ImageSource) GetValue(VolumeImageProperty); }
            set { SetValue(VolumeImageProperty, value); }
        }

        public double BarSize
        {
            get { return (double) GetValue(BarSizeProperty); }
            set
            {
                SetValue(BarSizeProperty, value);
                OnPropertyChanged();
                OnPropertyChanged("HalfBarSize");
                OnPropertyChanged("DoubleBarSize");

            }
        }

        public double KnobSize
        {
            get { return (double) GetValue(KnobSizeProperty); }
            set
            {
                SetValue(KnobSizeProperty, value);
                OnPropertyChanged();
                OnPropertyChanged("HalfKnobSize");
            }
        }


        public long VolumeStep
        {
            get { return (long) sbar.LargeChange; }
            set { sbar.LargeChange = value; }
        }

        public CornerRadius BottomCorner => new CornerRadius(BarSize / 2, BarSize / 2, 0, 0);
        public CornerRadius TopCorner => new CornerRadius(0, 0, BarSize / 2, BarSize / 2);
        public double HalfBarSize => BarSize / 2;
        public double HalfKnobSize => KnobSize / 2;
        public double DoubleBarSize => BarSize * 2;

        public double VolumeLevel
        {
            get { return _volume; }
            set
            {
                if (value != 0 && IsMuted || value == 0 && !IsMuted)
                    SetImage(value == 0);
                _volume = value;
                sbar.Value = value;
                OnPropertyChanged();
                OnPropertyChanged("IsMuted");
            }
        }

        public bool IsMuted => _volume == 0;

        public event PropertyChangedEventHandler PropertyChanged;

        public event Delegates.VolumeChangeHandler VolumeChanged;

        private void Volume_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            OnPropertyChanged("TopPadding");
        }

        private void SetImage(bool value)
        {
            but.Image = value ? MuteImage : VolumeImage;
        }

        private void Volume_MouseEnter(object sender, MouseEventArgs e)
        {
            IsInside = true;
            _timer?.Dispose();
            _timer = null;
        }

        private void Volume_MouseLeave(object sender, MouseEventArgs e)
        {
            IsInside = false;
            if (SliderIsShown)
            {
                _timer?.Dispose();
                _timer = new Timer(Timeout, null, ShowTimeout, TimeSpan.FromMilliseconds(-1));
            }
        }

        private void Timeout(object obj)
        {
            _timer?.Dispose();
            _timer = null;
            Dispatcher.Invoke(() => { SliderIsShown = false; });
        }

        private void But_Click(object sender, RoutedEventArgs e)
        {
            SliderIsShown = !SliderIsShown;
        }

        private void Sbar_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            VolumeLevel = sbar.Value;
            VolumeChanged?.Invoke(sbar.Value);
        }

        [NotifyPropertyChangedInvocator]
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}