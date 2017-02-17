using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using NutzCode.MPVPlayer.WPF.Wrapper.Annotations;

namespace NutzCode.MPVPlayer.WPF.Wrapper.Controls
{
    public partial class ControlBar : UserControl, INotifyPropertyChanged
    {
        public static readonly DependencyProperty SeekBarColorProperty = DependencyProperty.Register("SeekBarColor", typeof(Brush), typeof(ControlBar), new FrameworkPropertyMetadata(new SolidColorBrush(Color.FromArgb(0xFF, 0x00, 0x80, 0xd0))));
        public static readonly DependencyProperty MouseOverColorProperty = DependencyProperty.Register("MouseOverColor", typeof(Brush), typeof(ControlBar), new FrameworkPropertyMetadata(new SolidColorBrush(Color.FromArgb(0xFF, 0x00, 0x80, 0xd0))));
        public static readonly DependencyProperty SeekBarBackColorProperty = DependencyProperty.Register("SeekBarBackColor", typeof(Brush), typeof(ControlBar), new FrameworkPropertyMetadata(new SolidColorBrush(Color.FromArgb(0xFF, 0x00, 0x00, 0x00))));
        public static readonly DependencyProperty BackgroundColorProperty = DependencyProperty.Register("BackgroundColor", typeof(Brush), typeof(ControlBar), new FrameworkPropertyMetadata(new SolidColorBrush(Color.FromArgb(0xFF, 0x30, 0x30, 0x30))));
        public static readonly DependencyProperty PaddingRelationProperty = DependencyProperty.Register("PaddingRelation", typeof(Size), typeof(ControlBar), new FrameworkPropertyMetadata(new Size(0.125, 0.125)));
        public static readonly DependencyProperty HasExitProperty = DependencyProperty.Register("HasExit", typeof(bool), typeof(ControlBar), new FrameworkPropertyMetadata(true));
        public static readonly DependencyProperty HasAudiosProperty = DependencyProperty.Register("HasAudios", typeof(bool), typeof(ControlBar), new FrameworkPropertyMetadata(true));
        public static readonly DependencyProperty HasSubsProperty = DependencyProperty.Register("HasSubs", typeof(bool), typeof(ControlBar), new FrameworkPropertyMetadata(true));
        public static readonly DependencyProperty HasMaximizeProperty = DependencyProperty.Register("HasMaximize", typeof(bool), typeof(ControlBar), new FrameworkPropertyMetadata(true));
        public static readonly DependencyProperty ControlBarSizeProperty = DependencyProperty.Register("ControlBarSize", typeof(double), typeof(ControlBar), new FrameworkPropertyMetadata((double) 48.0));
        public static readonly DependencyProperty SeekbarKnobRelationProperty = DependencyProperty.Register("SeekbarKnobRelation", typeof(double), typeof(ControlBar), new FrameworkPropertyMetadata((double) 0.41666));
        public static readonly DependencyProperty SeekbarBarRelationProperty = DependencyProperty.Register("SeekbarBarRelation", typeof(double), typeof(ControlBar), new FrameworkPropertyMetadata((double) .25));
        public static readonly DependencyProperty VolumeKnobRelationProperty = DependencyProperty.Register("VolumeKnobRelation", typeof(double), typeof(ControlBar), new FrameworkPropertyMetadata((double) .291666));
        public static readonly DependencyProperty VolumeBarRelationProperty = DependencyProperty.Register("VolumeBarRelation", typeof(double), typeof(ControlBar), new FrameworkPropertyMetadata((double) 0.16666));
        public static readonly DependencyProperty VolumeHeightRelationProperty = DependencyProperty.Register("VolumeHeightRelation", typeof(double), typeof(ControlBar), new FrameworkPropertyMetadata((double) 3.0));
        public static readonly DependencyProperty TimeBarRelationProperty = DependencyProperty.Register("TimeBarRelation", typeof(double), typeof(ControlBar), new FrameworkPropertyMetadata((double) 1.3333));
        public static readonly DependencyProperty FontSizeRelationProperty = DependencyProperty.Register("FontSizeRelation", typeof(double), typeof(ControlBar), new FrameworkPropertyMetadata((double) 0.3333));
        public static readonly DependencyProperty TimeBarPaddingRelationProperty = DependencyProperty.Register("TimeBarPaddingRelation", typeof(double), typeof(ControlBar), new FrameworkPropertyMetadata((double) 0.125));


        public ControlBar()
        {
            InitializeComponent();
            this.Background = new SolidColorBrush(Colors.Transparent);
            Seekbar.TimePositionChanged += Seekbar_OnTimePositionChange;
            Seekbar.PositionChanged += (position) => PositionChanged?.Invoke(position);
            Volume.VolumeChanged += (volume) => VolumeChanged?.Invoke(volume);
            Play.PlayStateChanged += (play) => PlayStateChanged?.Invoke(play);
            Maximize.Clicked += () => MaximizeClicked?.Invoke();
            Menu.AudioClicked += () => AudiosClicked?.Invoke();
            Menu.SubtitlesClicked += () => SubtitlesClicked?.Invoke();
            Menu.ExitClicked += () => ExitClicked?.Invoke();
            Menu.ConfigClicked += () => ConfigClicked?.Invoke();
            this.MouseLeave += ControlBar_MouseLeave;
            this.MouseEnter += ControlBar_MouseEnter;
        }

        public bool IsInside { get; private set; }
        public int TimeFontSize => (int) (FontSizeRelation * ControlBarSize);
        public Thickness TimeBarPadding => new Thickness(0, 0, 0, (int) TimeBarPaddingRelation * ControlBarSize);
        public double TimeBarSize => TimeBarRelation * ControlBarSize;
        public double SeekBarBarSize => SeekbarBarRelation * ControlBarSize;
        public double SeekBarKnobSize => SeekbarKnobRelation * ControlBarSize;
        public double VolumeBarSize => VolumeBarRelation * ControlBarSize;
        public double VolumeKnobSize => VolumeKnobRelation * ControlBarSize;
        public double VolumeHeight => VolumeHeightRelation * ControlBarSize;

        public double FontSizeRelation
        {
            get { return (double) GetValue(FontSizeRelationProperty); }
            set { SetValue(FontSizeRelationProperty, value); }
        }

        public double TimeBarPaddingRelation
        {
            get { return (double) GetValue(TimeBarPaddingRelationProperty); }
            set { SetValue(TimeBarPaddingRelationProperty, value); }
        }

        public double TimeBarRelation
        {
            get { return (double) GetValue(TimeBarRelationProperty); }
            set { SetValue(TimeBarRelationProperty, value); }
        }

        public double SeekbarBarRelation
        {
            get { return (double) GetValue(SeekbarBarRelationProperty); }
            set { SetValue(SeekbarBarRelationProperty, value); }
        }

        public double SeekbarKnobRelation
        {
            get { return (double) GetValue(SeekbarKnobRelationProperty); }
            set { SetValue(SeekbarKnobRelationProperty, value); }
        }

        public double VolumeBarRelation
        {
            get { return (double) GetValue(VolumeBarRelationProperty); }
            set { SetValue(VolumeBarRelationProperty, value); }
        }

        public double VolumeHeightRelation
        {
            get { return (double) GetValue(VolumeHeightRelationProperty); }
            set { SetValue(VolumeHeightRelationProperty, value); }
        }

        public double VolumeKnobRelation
        {
            get { return (double) GetValue(VolumeKnobRelationProperty); }
            set { SetValue(VolumeKnobRelationProperty, value); }
        }

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

        public bool HasMaximize
        {
            get { return (bool) GetValue(HasMaximizeProperty); }
            set
            {
                SetValue(HasMaximizeProperty, value);
                OnPropertyChanged("MaximizeSize");
            }
        }

        public bool IsPlaying
        {
            get { return Play.IsPlaying; }
            set { Play.IsPlaying = value; }
        }

        public Brush SeekBarColor
        {
            get { return (Brush) GetValue(SeekBarColorProperty); }
            set { SetValue(SeekBarColorProperty, value); }
        }

        public double ControlBarSize
        {
            get { return (double) GetValue(ControlBarSizeProperty); }
            set
            {
                SetValue(ControlBarSizeProperty, value);
                OnPropertyChanged();
                OnPropertyChanged("TimeFontSize");
                OnPropertyChanged("TimeBarPadding");
                OnPropertyChanged("TimeBarSize");
                OnPropertyChanged("SeekBarBarSize");
                OnPropertyChanged("SeekBarKnobSize");
                OnPropertyChanged("VolumeBarSize");
                OnPropertyChanged("VolumeKnobSize");
                OnPropertyChanged("VolumeHeight");
                OnPropertyChanged("MaximizeSize");
            }
        }

        public Brush BackgroundColor
        {
            get { return (Brush) GetValue(BackgroundColorProperty); }
            set { SetValue(BackgroundColorProperty, value); }
        }

        public Brush SeekBarBackColor
        {
            get { return (Brush) GetValue(SeekBarBackColorProperty); }
            set { SetValue(SeekBarBackColorProperty, value); }
        }

        public Brush MouseOverColor
        {
            get { return (Brush) GetValue(MouseOverColorProperty); }
            set { SetValue(MouseOverColorProperty, value); }
        }

        public double PaddingRelation
        {
            get { return (double) GetValue(PaddingRelationProperty); }
            set { SetValue(PaddingRelationProperty, value); }
        }


        public long Position
        {
            get { return Seekbar.Position; }
            set
            {
                if (value > Seekbar.MaxPosition)
                    Seekbar.MaxPosition = value;
                Seekbar.Position = value;
            }
        }

        public long Length
        {
            get { return Seekbar.MaxPosition; }
            set { Seekbar.MaxPosition = value; }
        }

        public long SeekBarChange
        {
            get { return Seekbar.BarStep; }
            set { Seekbar.BarStep = value; }
        }


        public double VolumeLevel
        {
            get { return Volume.VolumeLevel; }
            set { Volume.VolumeLevel = value; }
        }

        public long VolumeChange
        {
            get { return Volume.VolumeStep; }
            set { Volume.VolumeStep = value; }
        }

        public double MaximizeSize => HasMaximize ? ControlBarSize : 0;

        public event PropertyChangedEventHandler PropertyChanged;


        public event Delegates.PlayStateChangeHandler PlayStateChanged;
        public event Delegates.PositionChangeHandler PositionChanged;
        public event Delegates.VolumeChangeHandler VolumeChanged;
        public event Delegates.ButtonChangeHandler MaximizeClicked;
        public event Delegates.ButtonChangeHandler ExitClicked;
        public event Delegates.ButtonChangeHandler ConfigClicked;
        public event Delegates.ButtonChangeHandler SubtitlesClicked;
        public event Delegates.ButtonChangeHandler AudiosClicked;


        private void ControlBar_MouseEnter(object sender, MouseEventArgs e)
        {
            IsInside = true;
        }

        private void ControlBar_MouseLeave(object sender, MouseEventArgs e)
        {
            IsInside = false;
        }


        private void Seekbar_OnTimePositionChange(long position)
        {
            Lefttime.Position = Seekbar.TimePosition;
            Righttime.Position = Seekbar.NegativeTimePosition;
        }


        public void SetVisibility(Visibility v, bool nofade = false)
        {
            if (nofade)
                Visibility = v;
            else
                this.VisbilityWithFade(v);
        }

        [NotifyPropertyChangedInvocator]
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}