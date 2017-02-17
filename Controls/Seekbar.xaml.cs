using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using NutzCode.MPVPlayer.WPF.Wrapper.Annotations;
// ReSharper disable CompareOfFloatsByEqualityOperator

// ReSharper disable RedundantExtendsListEntry

namespace NutzCode.MPVPlayer.WPF.Wrapper.Controls
{
    public partial class Seekbar : UserControl, INotifyPropertyChanged
    {
        public static readonly DependencyProperty TimePositionProperty = DependencyProperty.Register("TimePosition", typeof(long), typeof(Seekbar));
        public static readonly DependencyProperty NegativeTimePositionProperty = DependencyProperty.Register("NegativeTimePosition", typeof(long), typeof(Seekbar));
        public static readonly DependencyProperty PositionProperty = DependencyProperty.Register("Position", typeof(long), typeof(Seekbar));
        public static readonly DependencyProperty KnobImageProperty = DependencyProperty.Register("KnobImage", typeof(ImageSource), typeof(Seekbar), new FrameworkPropertyMetadata(new BitmapImage(new Uri(@"pack://application:,,,/NutzCode.MPVPlayer.WPF.Wrapper;component/images/knob.png", UriKind.RelativeOrAbsolute))));
        public static readonly DependencyProperty LeftBarColorProperty = DependencyProperty.Register("LeftBarColor", typeof(Brush), typeof(Seekbar), new FrameworkPropertyMetadata(new SolidColorBrush(Color.FromArgb(0xFF, 0x00, 0x80, 0xd0))));
        public static readonly DependencyProperty RightBarColorProperty = DependencyProperty.Register("RightBarColor", typeof(Brush), typeof(Seekbar), new FrameworkPropertyMetadata(new SolidColorBrush(Color.FromArgb(0xFF, 0x00, 0x00, 0x00))));
        public static readonly DependencyProperty BarSizeProperty = DependencyProperty.Register("BarSize", typeof(double), typeof(Seekbar), new FrameworkPropertyMetadata(10.0D));
        public static readonly DependencyProperty KnobSizeProperty = DependencyProperty.Register("KnobSize", typeof(double), typeof(Seekbar), new FrameworkPropertyMetadata(20.0D));

        private bool _clamp;


        private bool _drag;

        public Seekbar()
        {
            InitializeComponent();
            sbar.ValueChanged += Sbar_ValueChanged;
            sbar.GotMouseCapture += Sbar_GotMouseCapture;
            sbar.LostMouseCapture += Sbar_LostMouseCapture;
            sbar.SmallChange = 30;
            sbar.LargeChange = 30;
            sbar.Maximum = 35 * 3600;
        }

        public Brush LeftBarColor
        {
            get { return (Brush) GetValue(LeftBarColorProperty); }
            set { SetValue(LeftBarColorProperty, value); }
        }

        public Brush RightBarColor
        {
            get { return (Brush) GetValue(RightBarColorProperty); }
            set { SetValue(RightBarColorProperty, value); }
        }

        public ImageSource KnobImage
        {
            get { return (ImageSource) GetValue(KnobImageProperty); }
            set { SetValue(KnobImageProperty, value); }
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
                OnPropertyChanged("LeftCorner");
                OnPropertyChanged("RightCorner");
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

        public long BarStep
        {
            get { return (long) sbar.LargeChange; }
            set { sbar.LargeChange = value; }
        }

        public CornerRadius LeftCorner => new CornerRadius(BarSize / 2, 0, 0, BarSize / 2);
        public CornerRadius RightCorner => new CornerRadius(0, BarSize / 2, BarSize / 2, 0);
        public double HalfBarSize => BarSize / 2;
        public double HalfKnobSize => KnobSize / 2;
        public double DoubleBarSize => BarSize * 2;

        public long MaxPosition
        {
            get { return (long) sbar.Maximum; }
            set
            {
                sbar.Maximum = value;
                TimePosition = (long) sbar.Value;
                NegativeTimePosition = value - (long) sbar.Value;
                TimePositionChanged?.Invoke((long) sbar.Value);
            }
        }

        public long Position
        {
            get
            {
                if (_drag)
                    return (long) GetValue(PositionProperty);
                return (long) sbar.Value;
            }
            set
            {
                if (sbar.Value != value && _drag == false)
                {
                    _clamp = true;
                    SetValue(PositionProperty, value);
                    sbar.Value = value;
                    OnPropertyChanged();
                    TimePosition = value;
                    NegativeTimePosition = MaxPosition - value;
                    TimePositionChanged?.Invoke((long) sbar.Value);
                }
            }
        }

        public long NegativeTimePosition
        {
            get { return (long) GetValue(NegativeTimePositionProperty); }
            set
            {
                SetValue(NegativeTimePositionProperty, value);
                OnPropertyChanged();
            }
        }

        public long TimePosition
        {
            get { return (long) GetValue(TimePositionProperty); }
            set
            {
                SetValue(TimePositionProperty, value);
                OnPropertyChanged();
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        public event Delegates.PositionChangeHandler PositionChanged;
        public event Delegates.PositionChangeHandler TimePositionChanged;

        private void Sbar_LostMouseCapture(object sender, MouseEventArgs e)
        {
            _drag = false;
            Sbar_ValueChanged(this, null);
        }

        private void Sbar_GotMouseCapture(object sender, MouseEventArgs e)
        {
            _drag = true;
        }

        private void Sbar_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            if (_clamp)
            {
                _clamp = false;
                return;
            }
            if (_drag)
            {
                TimePosition = (long) sbar.Value;
                NegativeTimePosition = MaxPosition - (long) sbar.Value;
                TimePositionChanged?.Invoke((long) sbar.Value);
            }
            else
            {
                SetValue(PositionProperty, (long) sbar.Value);
                TimePositionChanged?.Invoke((long) sbar.Value);
                PositionChanged?.Invoke((long) sbar.Value);
            }
        }

        [NotifyPropertyChangedInvocator]
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}