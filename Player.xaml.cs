using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Runtime.InteropServices;
using System.Text.RegularExpressions;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Interop;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Threading;
using NutzCode.MPVPlayer.WPF.Wrapper.Models;
// ReSharper disable RedundantExtendsListEntry
// ReSharper disable CompareOfFloatsByEqualityOperator

namespace NutzCode.MPVPlayer.WPF.Wrapper
{
    /// <summary>
    /// Interaction logic for PlayerForm.xaml
    /// </summary>
    public partial class Player : UserControl
    {
        public static readonly DependencyProperty UseBorderlessWindowProperty = DependencyProperty.Register("UseBorderlessWindow", typeof(bool), typeof(Player), new FrameworkPropertyMetadata(false, FrameworkPropertyMetadataOptions.None));
        public static readonly DependencyProperty ResizeBorderSizeProperty = DependencyProperty.Register("ResizeBorderSize", typeof(int), typeof(Player), new FrameworkPropertyMetadata(4, FrameworkPropertyMetadataOptions.None));

        private int _inc;
        private PlayRequest _playRequest;
        private PlayerSettings _settings;
        private Streams _audioForm;
        private Streams _subsForm;
        private Settings _settingsForm;

        private readonly List<Device> _devices = new List<Device>();
        private double _buffering;
        private Point _lastpoint = new Point(-100, -100);
        private int _lastpos = -1;
        private string _lastsid = "auto";
        private bool _mSingleClick;
        private bool _loading;
        private int _prevvolume;
        private string _sshot;
        private Timer _clickTimer;
        private Timer _periodicTimer;
        private Timer _timer;

        private readonly Regex _rname = new Regex("\"name\":\"(.*?)\".*?\"description\":\"(.*?)\"", RegexOptions.Singleline | RegexOptions.Compiled);
        public ObservableCollection<Stream> Audios = new ObservableCollection<Stream>();
        public ObservableCollection<Stream> Subtitles = new ObservableCollection<Stream>();
        public ObservableCollection<Stream> Videos = new ObservableCollection<Stream>();
        private Window _window;


        public Player()
        {
            InitializeComponent();

            Bar.PlayStateChanged += Bar_PlayStateChanged;
            Bar.PositionChanged += Bar_PositionChanged;
            Bar.VolumeChanged += Bar_VolumeChanged;
            Bar.MaximizeClicked += SwitchStates;
            Bar.ExitClicked += Close;
            Bar.SubtitlesClicked += Bar_SubsClicked;
            Bar.AudiosClicked += Bar_AudiosClicked;
            Bar.ConfigClicked += Bar_ConfigClicked;
            SizeChanged += Player_SizeChanged;
            Loading.Percentage = "";
        }


        public bool UseBorderlessWindow
        {
            get { return (bool)GetValue(UseBorderlessWindowProperty); }
            set { SetValue(UseBorderlessWindowProperty, value); }
        }

        public int ResizeBorderSize
        {
            get { return (int)GetValue(ResizeBorderSizeProperty); }
            set { SetValue(ResizeBorderSizeProperty, value); }
        }

        public long Duration { get; private set; } 
        public int VideoWidth { get; private set; }
        public int VideoHeight { get; private set; }

        public bool IsPlaying => Bar.IsPlaying;

        public int Volume
        {
            get { return MPVInterop.Instance.GetIntProperty("volume"); }
            set
            {
                var v1 = (double) value / 100;
                Bar.VolumeLevel = v1;
                Dispatcher.Invoke(() => { MPVInterop.Instance.SetProperty("volume", value.ToString(CultureInfo.InvariantCulture)); });
            }
        }

        public double Time
        {
            get { return MPVInterop.Instance.GetDoubleProperty("time-pos"); }
            set { MPVInterop.Instance.DoMpvCommand("seek", value.ToString(CultureInfo.InvariantCulture), "absolute"); }
        }

        public TimeSpan DisappearTimeout { get; set; } = TimeSpan.FromSeconds(3);


        public event Delegates.PlayStateChangeHandler PlayStateChanged;
        public event Delegates.PositionChangeHandler PositionChanged;
        public event Delegates.VolumeChangeHandler VolumeChanged;
        public event Delegates.SettingsChangedHandler SettingsChange;

        public void SetTopControl(UIElement topcontrol)
        {
            topcontrol.PreviewMouseLeftButtonDown += Topcontrol_PreviewMouseLeftButtonDown;
            topcontrol.MouseMove += Topcontrol_MouseMove;
            topcontrol.MouseDown += Topcontrol_MouseDown;
            topcontrol.KeyDown += Topcontrol_KeyDown;
            if (topcontrol is Window)
            {
                _window = topcontrol as Window;
                _window.Closed += Window_Closed;
                Bar.HasMaximize = true;
                Bar.HasExit = true;
            }
            else
            {
                Bar.HasMaximize = false;
                Bar.HasExit = false;
            }
        }

        private void Topcontrol_KeyDown(object sender, KeyEventArgs e)
        {
            ProcessKey(e);
        }

        private void Window_Closed(object sender, EventArgs e)
        {
            Stop();
            MPVInterop.Instance.Finish();
            VideoPlayer.Dispose();
        }

        [DllImport("user32.dll", CharSet = CharSet.Auto)]
        private static extern IntPtr SendMessage(IntPtr hWnd, uint msg, IntPtr wParam, IntPtr lParam);

        [DllImport("user32.dll")]
        private static extern uint GetDoubleClickTime();

        private int BorderPosition(Point p)
        {
            var lf = 0;
            if (p.X >= 0 && p.X < ResizeBorderSize)
                lf = 1;
            else if (p.X > _window.ActualWidth - ResizeBorderSize && p.X < _window.ActualWidth)
                lf = 2;
            if (p.Y >= 0 && p.Y < ResizeBorderSize)
                lf += 3;
            else if (p.Y > _window.ActualHeight - ResizeBorderSize && p.Y < _window.ActualHeight)
                lf += 6;
            return lf;
        }

        private void Topcontrol_PreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (UseBorderlessWindow && _window != null && _window.WindowState != WindowState.Maximized && _window.WindowStyle == WindowStyle.None && _window.ResizeMode == ResizeMode.NoResize)
            {
                Point p = e.GetPosition(_window);
                var bp = BorderPosition(p);
                if (bp != 0)
                {
                    bp += 0xF000;
                    HwndSource hwndSource = PresentationSource.FromVisual((Visual) sender) as HwndSource;
                    if (hwndSource != null)
                    {
                        SendMessage(hwndSource.Handle, 0x112, (IntPtr) bp, IntPtr.Zero);
                        e.Handled = true;
                    }
                }
            }
            OnPreviewMouseLeftButtonDown(e);
        }

        public void Topcontrol_MouseMove(object sender, MouseEventArgs e)
        {
            if (UseBorderlessWindow && _window != null && _window.WindowState != WindowState.Maximized && _window.WindowStyle == WindowStyle.None && _window.ResizeMode == ResizeMode.NoResize)
            {
                Point pn = e.GetPosition(_window);
                if (!(pn.X == 0 && pn.Y == 0 && Math.Abs(pn.X - _lastpoint.X) > 10 && Math.Abs(pn.Y - _lastpoint.Y) > 10))
                {
                    _lastpoint = pn;
                    var bp = BorderPosition(pn);
                    if (bp != _lastpos)
                    {
                        _lastpos = bp;
                        switch (bp)
                        {
                            case 0:
                                Mouse.OverrideCursor = Cursors.Arrow;
                                break;
                            case 1:
                            case 2:
                                Mouse.OverrideCursor = Cursors.SizeWE;
                                break;
                            case 3:
                            case 6:
                                Mouse.OverrideCursor = Cursors.SizeNS;
                                break;
                            case 4:
                            case 8:
                                Mouse.OverrideCursor = Cursors.SizeNWSE;
                                break;
                            case 5:
                            case 7:
                                Mouse.OverrideCursor = Cursors.SizeNESW;
                                break;
                        }
                    }
                }
            }
            else if (UseBorderlessWindow && _window != null && _window.WindowState == WindowState.Maximized && _lastpos != 0)
            {
                _lastpos = 0;
                Mouse.OverrideCursor = Cursors.Arrow;
            }
            if (!Bar.IsPlaying)
                return;
            Point p = e.GetPosition(this);
            CheckPosition(p);
        }

        private void Topcontrol_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (Bar.IsInside)
                return;
            if (_window != null)
                _window.DragMove();
            if (e.ClickCount < 2)
            {
                _mSingleClick = true;
                _clickTimer = new Timer(ClickTimer_Elapsed, null, GetDoubleClickTime(), System.Threading.Timeout.Infinite);
            }
            else if (e.ClickCount == 2)
            {
                _clickTimer?.Dispose();
                _clickTimer = null;
                if (_window != null)
                {
                    Point p = e.GetPosition(_window);
                    if (p.Y < 30 && p.X > _window.ActualWidth - 30)
                        Close();
                }
                _mSingleClick = false;
                SwitchStates();
            }
        }

        private void ClickTimer_Elapsed(object val)
        {
            if (_mSingleClick)
            {
                _clickTimer?.Dispose();
                _clickTimer = null;
                _mSingleClick = false;
                if (IsPlaying)
                    Play();
                else
                    Pause();
            }
        }

        private void SwitchStates()
        {
            if (_window != null)
                if (_window.WindowState == WindowState.Maximized)
                    Normal();
                else
                    Maximize();
        }

        private void Maximize()
        {
            if (_window != null && _window.WindowState != WindowState.Maximized)
            {
                _window.ResizeMode = ResizeMode.CanResize; //Hack to make it work windowChrome and the adorners.
                _window.WindowState = WindowState.Maximized;
            }
        }

        private void Minimize()
        {
            if (_window != null && _window.WindowState != WindowState.Minimized)
            {
                _window.ResizeMode = ResizeMode.NoResize; //Hack to make it work windowChrome and the adorners.
                _window.WindowState = WindowState.Minimized;
            }
        }

        private void Normal()
        {
            if (_window != null && _window.WindowState != WindowState.Normal)
            {
                _window.ResizeMode = ResizeMode.NoResize; //Hack to make it work windowChrome and the adorners.
                _window.WindowState = WindowState.Normal;
            }
        }

        internal void StartWatcher()
        {
            StopWatcher();
            Application.Current.Dispatcher.ShutdownStarted += OnShutdownStarted;
            _periodicTimer = new Timer(InfoTimer, null, 0, 50);
        }

        internal void StopWatcher()
        {
            _periodicTimer?.Dispose();
            _periodicTimer = null;
        }

        public void Play(PlayRequest p, PlayerSettings settings)
        {
            _playRequest = p;
            if (settings.KeyBindings == null)
                settings.KeyBindings = KeyBindings.Default;
            if (!string.IsNullOrEmpty(p.PreviewImageSourceUri))
            {
                Preview.Source = new BitmapImage(new Uri(p.PreviewImageSourceUri));
                Preview.Visibility = Visibility.Visible;
            }
            MPVInterop.Instance.Initialize(VideoPlayer);
            _settings = null;
            SendSettings(settings);
            _settings = settings;
            if (p.IsPlaylist)
            {
                MPVInterop.Instance.DoMpvCommand("loadlist", p.Uri);
            }
            else
            {
                MPVInterop.Instance.DoMpvCommand("loadfile", p.Uri);
                if (p.ExternalSubtitles != null)
                    foreach (var n in p.ExternalSubtitles)
                        MPVInterop.Instance.DoMpvCommand("sub-add", n);
                if (p.ExternalAudios != null)
                    foreach (var n in p.ExternalAudios)
                        MPVInterop.Instance.DoMpvCommand("audio-add", n);
            }


            if (!p.Autoplay)
                MPVInterop.Instance.MPVPause();
            StartWatcher();
            Bar.IsPlaying = p.Autoplay;
            PlayStateChanged?.Invoke(Bar.IsPlaying);
        }

        public string TakeScreenshot(long position = 0)
        {
            var tempfile = Path.GetTempFileName() + ".jpg";
            tempfile = tempfile.Replace(".tmp.", ".");
            if (position != 0)
            {
                var oldPosition = Time;
                Time = position;
                string w;
                do
                {
                    Thread.Sleep(10);
                    w = MPVInterop.Instance.GetProperty("seeking");
                } while (w == null || w == "yes");
                MPVInterop.Instance.DoMpvCommand("screenshot-to-file", tempfile, "video");
                Time = oldPosition;
            }
            else
            {
                MPVInterop.Instance.DoMpvCommand("screenshot-to-file", "\"" + tempfile + "\"", "video");
            }
            return tempfile;
        }

        public void Stop()
        {
            Pause();
            StopWatcher();
        }


        public void GetInfo()
        {
            Duration = (long) MPVInterop.Instance.GetDoubleProperty("duration");
            VideoWidth = MPVInterop.Instance.GetIntProperty("width");
            VideoHeight = MPVInterop.Instance.GetIntProperty("height");
            var streamcount = MPVInterop.Instance.GetIntProperty("track-list/count");
            var devices = MPVInterop.Instance.GetProperty("audio-device-list");
            MatchCollection mc = _rname.Matches(devices);
            foreach (Match m in mc)
                if (m.Success)
                    if (m.Groups[1].Value != "auto")
                    {
                        Device d = new Device {Name = m.Groups[1].Value, Description = m.Groups[2].Value};
                        _devices.Add(d);
                    }
            Subtitles.Clear();
            Audios.Clear();
            Videos.Clear();
            for (var x = 0; x < streamcount; x++)
            {
                var id = MPVInterop.Instance.GetIntProperty("track-list/" + x + "/id");
                var type = MPVInterop.Instance.GetProperty("track-list/" + x + "/type");
                var title = MPVInterop.Instance.GetProperty("track-list/" + x + "/title");
                var lang = MPVInterop.Instance.GetProperty("track-list/" + x + "/lang");
                var external = MPVInterop.Instance.GetProperty("track-list/" + x + "/external-filename");
                var selected = MPVInterop.Instance.GetProperty("track-list/" + x + "/selected");
                Stream s = new Stream();
                s.Id = id;
                s.Title = title;
                s.Language = lang;
                s.ExternalFilename = external;
                s.Selected = selected != null && selected == "yes";
                if (type == "audio")
                    Audios.Add(s);
                else if (type == "sub")
                    Subtitles.Add(s);
                else if (type == "video")
                    Videos.Add(s);
            }
            if (_playRequest.TakeScreenshotOnStart)
            {
                var dur = Duration / 100D * _playRequest.ScreenshotTimePercentage;
                _sshot = TakeScreenshot((long) dur);
                if (!IsPlaying)
                    Dispatcher.Invoke(() =>
                    {
                        Preview.Source = new BitmapImage(new Uri(_sshot));
                        Preview.Visibility = Visibility.Visible;
                    });
            }
            if (_playRequest.ResumePosition != 0)
                Time = _playRequest.ResumePosition;
        }

        public string GetTakedScreenshot()
        {
            return _sshot;
        }

        private void InfoTimer(object obj)
        {
            var w = MPVInterop.Instance.GetProperty("seeking");
            if (!string.IsNullOrEmpty(w) && w == "no")
            {
                _periodicTimer.Dispose();
                GetInfo();
                _periodicTimer = new Timer(SecondTimer, null, 0, 100);
                Dispatcher.Invoke(() =>
                {
                    if (IsPlaying)
                        Preview.Visibility = Visibility.Collapsed;
                    Loading.Visibility = Visibility.Collapsed;
                    Window win = GetTopParent();
                    _subsForm = new Streams("Subtitles", true);
                    _subsForm.Owner = win;
                    _subsForm.StreamChanged += SubsForm_StreamChanged;
                    _audioForm = new Streams("Audios", false);
                    _audioForm.StreamChanged += AudioForm_StreamChanged;
                    _audioForm.Owner = win;
                    _settingsForm = new Settings(_settings, _devices);
                    _settingsForm.SettingsChanged += Settings_SettingsChanged;
                    _settingsForm.Owner = win;
                    Bar.HasAudios = Audios.Count > 1;
                    Bar.HasSubs = Subtitles.Count > 0;
                    Bar.Length = Duration;
                    Resize(VideoWidth, VideoHeight);
                });
            }
        }

        private void Settings_SettingsChanged(PlayerSettings pl)
        {
            SendSettings(pl);
            SettingsChange?.Invoke(pl);
        }


        private void SecondTimer(object obj)
        {
            bool shouldDispatch = false;
            Visibility loadingVis = Visibility.Visible;
            string buffering = string.Empty;
            bool cachepause = MPVInterop.Instance.GetProperty("paused-for-cache") == "yes";
            bool seekpause= MPVInterop.Instance.GetProperty("seeking") == "yes";
            if ((!_loading && cachepause) || (!_loading && seekpause))
            {
                _inc = 0;
                _loading = true;
            }
            if (!seekpause && !cachepause)
            {
                _loading = false;
                loadingVis = Visibility.Collapsed;
                shouldDispatch = true;
            }
            if (_inc % 10 == 0)
            {

                shouldDispatch = true;
                if (_loading && cachepause)
                    buffering = MPVInterop.Instance.GetDoubleProperty("cache-buffering-state") + "%";
                _inc = 0;
            }
            _inc++;

            if (shouldDispatch)
            {
                Dispatcher.Invoke(() =>
                {
                    if (Duration != 0)
                        Bar.Position = (long)Time;
                    Loading.Visibility = loadingVis;
                    Loading.Percentage = buffering;
                });
            }       
        }

        private void Player_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            if ((e.NewSize.Width <= 320 || e.NewSize.Height <= 240) && Bar.ControlBarSize != 32)
                Bar.ControlBarSize = 32;
            else if (e.NewSize.Width >= 320 && e.NewSize.Height >= 240 && Bar.ControlBarSize == 32)
                Bar.ControlBarSize = 48;
            if (e.NewSize.Width != ((FrameworkElement) VideoPlayer.Adornment).ActualWidth && ((FrameworkElement) VideoPlayer.Adornment).ActualWidth != 0)
            {
                Rect rect = new Rect(PointToScreen(new Point()),
                    PointToScreen(new Point(ActualWidth, ActualHeight))
                );

                VideoPlayer.GetHwndAdorner().UpdateOwnerPosition(rect);
            }
        }

        private void AudioForm_StreamChanged(Stream s)
        {
            Dispatcher.Invoke(() =>
            {
                if (s != null)
                    MPVInterop.Instance.SetProperty("aid", s.Id.ToString());
            });
        }

        private void SubsForm_StreamChanged(Stream s)
        {
            Dispatcher.Invoke(() =>
            {
                if (s != null)
                    _lastsid = s.Id.ToString();
                MPVInterop.Instance.SetProperty("sid", s?.Id.ToString() ?? "no");
            });
        }

        private void Bar_AudiosClicked()
        {
            _audioForm.DataContext = Audios;
            if (_audioForm.IsLoaded)
            {
                _audioForm.Visibility = Visibility.Visible;
                _audioForm.Activate();
            }
            else
            {
                _audioForm.Show();
            }
        }

        private void Bar_SubsClicked()
        {
            _subsForm.DataContext = Subtitles;
            if (_subsForm.IsLoaded)
            {
                _subsForm.Visibility = Visibility.Visible;
                _subsForm.Activate();
            }
            else
            {
                _subsForm.Show();
            }
        }

        private void Bar_ConfigClicked()
        {
            if (_settingsForm.IsLoaded)
            {
                _settingsForm.Visibility = Visibility.Visible;
                _settingsForm.Activate();
            }
            else
            {
                _settingsForm.Show();
            }
        }

        public void Close()
        {
            if (_window != null)
            {
                _window?.Close();
            }
            else
            {
                Stop();
                MPVInterop.Instance.Finish();
                VideoPlayer.Dispose();
            }
        }


        private void CheckPosition(Point p)
        {
            //if (p.Y >= ActualHeight- Bar.ControlBarSize)
            if (Bar.IsInside || p.Y >= ActualHeight - Bar.ControlBarSize)
            {
                CleanTimeout();
                if (Bar.Visibility != Visibility.Visible)
                    Bar.SetVisibility(Visibility.Visible);
            }
            else
            {
                SetDisappearTimeout();
            }
        }

        private void SetDisappearTimeout()
        {
            if (_timer == null && Bar.Visibility == Visibility.Visible)
                _timer = new Timer(Timeout, null, DisappearTimeout, TimeSpan.FromMilliseconds(-1));
        }

        private void CleanTimeout()
        {
            _timer?.Dispose();
            _timer = null;
        }

        private void Timeout(object o)
        {
            Dispatcher.Invoke(() =>
            {
                CleanTimeout();
                Bar.SetVisibility(Visibility.Hidden);
            });
        }

        private void Bar_PositionChanged(long position)
        {
            Time = position;
            PositionChanged?.Invoke(position);
        }

        private void Bar_VolumeChanged(double volume)
        {
            var value = (int) (volume * 100);
            Dispatcher.Invoke(() => { MPVInterop.Instance.SetProperty("volume", value.ToString(CultureInfo.InvariantCulture)); });
            VolumeChanged?.Invoke(volume);
        }

        private void Bar_PlayStateChanged(bool isplaying)
        {
            if (isplaying)
            {
                MPVInterop.Instance.MPVPlay();
                Dispatcher.Invoke(() =>
                {
                    if (Preview.Visibility == Visibility.Visible)
                        Preview.Visibility = Visibility.Collapsed;
                });
            }
            else
            {
                MPVInterop.Instance.MPVPause();
            }
            PlayStateChange(isplaying);
            PlayStateChanged?.Invoke(Bar.IsPlaying);
        }

        public void Play()
        {
            MPVInterop.Instance.MPVPlay();
            Dispatcher.Invoke(() =>
            {
                if (Preview.Visibility == Visibility.Visible)
                    Preview.Visibility = Visibility.Collapsed;
                Bar.IsPlaying = true;
                PlayStateChange(true);
            });
        }

        public void Pause()
        {
            MPVInterop.Instance.MPVPause();
            Dispatcher.Invoke(() =>
            {
                Bar.IsPlaying = false;
                PlayStateChange(false);
            });
        }

        private void PlayStateChange(bool play)
        {
            if (play)
            {
                Point p = Mouse.GetPosition(this);
                CheckPosition(p);
            }
            else
            {
                Bar.SetVisibility(Visibility.Visible);
            }
        }


        public void Resize(int width, int height)
        {
            Application.Current.Dispatcher.Invoke(DispatcherPriority.Normal, (Action) delegate
            {
                double rel = (double)width /  (double)height;
                double currentrel =  VideoPlayer.ActualWidth / VideoPlayer.ActualHeight;
                if (Math.Abs(currentrel - rel) > double.Epsilon)
                {
                    double h = VideoPlayer.ActualWidth / rel;
                    if (h <= VideoPlayer.ActualHeight)
                    {
                        double delta = VideoPlayer.ActualHeight - h;
                        if (_window != null)
                        {
                            _window.Height = ActualHeight - delta;
                            _window.Top = _window.Top + delta / 2;
                        }
                    }
                    else
                    {
                        double w = VideoPlayer.ActualHeight * rel;
                        double delta = VideoPlayer.ActualWidth - w;
                        if (_window != null)
                        {
                            _window.Width = _window.ActualWidth - delta;
                            _window.Left = _window.Left + delta / 2;
                        }
                    }
                }
            });
        }

        ~Player()
        {
            MPVInterop.Instance.Finish();
            VideoPlayer.Dispose();
        }

        private void OnShutdownStarted(object sender, EventArgs e)
        {
            MPVInterop.Instance.Finish();
        }

        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
        }

        private Window GetTopParent()
        {
            DependencyObject dpParent = Parent;
            do
            {
                dpParent = LogicalTreeHelper.GetParent(dpParent);
            } while (dpParent.GetType().BaseType != typeof(Window));

            return dpParent as Window;
        }

        private void SendSettings(PlayerSettings newSettings)
        {
            if (_settings == null || _settings.Device != newSettings.Device)
            {
                MPVInterop.Instance.SetProperty("audio-device", newSettings.Device);
                if (_settings != null)
                    _settings.Device = newSettings.Device;
            }
            if (_settings == null || _settings.HardwareDecoding != newSettings.HardwareDecoding)
            {
                MPVInterop.Instance.SetProperty("hwdec", newSettings.HardwareDecoding ? "auto" : "no");
                if (_settings != null)
                    _settings.HardwareDecoding = newSettings.HardwareDecoding;
            }
            if (_settings == null || _settings.AudioOutput != newSettings.AudioOutput)
            {
                MPVInterop.Instance.SetProperty("audio-channels", newSettings.AudioOutput);
                if (_settings != null)
                    _settings.AudioOutput = newSettings.AudioOutput;
            }
            if (_settings == null || _settings.Passthrough != newSettings.Passthrough)
            {
                MPVInterop.Instance.SetProperty("audio-spdif", newSettings.Passthrough);
                if (_settings != null)
                    _settings.Passthrough = newSettings.Passthrough;
            }
            if (_settings == null || _settings.NormalizeVolume != newSettings.NormalizeVolume)
            {
                MPVInterop.Instance.SetProperty("audio-normalize-downmix", newSettings.NormalizeVolume ? "yes" : "no");
                if (_settings != null)
                    _settings.NormalizeVolume = newSettings.NormalizeVolume;
            }
            if (_settings == null || _settings.ExclusiveMode != newSettings.ExclusiveMode)
            {
                MPVInterop.Instance.SetProperty("audio-exclusive", newSettings.ExclusiveMode ? "yes" : "no");
                if (_settings != null)
                    _settings.ExclusiveMode = newSettings.ExclusiveMode;
            }
            if (_settings == null || _settings.PreferredAudios != newSettings.PreferredAudios)
            {
                MPVInterop.Instance.SetProperty("alang", newSettings.PreferredAudios);
                if (_settings != null)
                    _settings.PreferredAudios = newSettings.PreferredAudios;
            }
            if (_settings == null || _settings.SubtitlesEnabled != newSettings.SubtitlesEnabled)
            {
                MPVInterop.Instance.SetProperty("sid", newSettings.SubtitlesEnabled ? _lastsid : "no");
                if (_settings != null)
                    _settings.SubtitlesEnabled = newSettings.SubtitlesEnabled;
            }
            if (_settings == null || _settings.SubtitleSize != newSettings.SubtitleSize)
            {
                MPVInterop.Instance.SetProperty("sub-scale", newSettings.SubtitleSize.ToString(CultureInfo.InvariantCulture));
                if (_settings != null)
                    _settings.SubtitleSize = newSettings.SubtitleSize;
            }
            if (_settings == null || _settings.PreferredSubtitles != newSettings.PreferredSubtitles)
            {
                MPVInterop.Instance.SetProperty("slang", newSettings.PreferredSubtitles);
                if (_settings != null)
                    _settings.PreferredSubtitles = newSettings.PreferredSubtitles;
            }
        }

        private void AddTime(int seconds)
        {
            var time = Time;
            time += seconds;
            if (time < 0)
                time = 0;
            if (time > Duration)
                time = Duration;
            Time = time;
        }

        private void AddVolume(int percent)
        {
            var val = Volume;
            val += percent;
            if (val < 0)
                val = 0;
            if (val > 100)
                val = 100;
            Volume = val;
        }

        public void ProcessKey(KeyEventArgs e)
        {
            KeyAction act = _settings.KeyBindings.Translate(e.Key);
            switch (act)
            {
                case KeyAction.None:
                    break;
                case KeyAction.PlayPauseToggle:
                    if (IsPlaying)
                        Pause();
                    else
                        Play();
                    break;
                case KeyAction.Play:
                    if (!IsPlaying)
                        Play();
                    break;
                case KeyAction.Pause:
                    if (IsPlaying)
                        Pause();
                    break;
                case KeyAction.Back15Seconds:
                    AddTime(-15);
                    break;
                case KeyAction.Back30Seconds:
                    AddTime(-30);
                    break;
                case KeyAction.Back1Minute:
                    AddTime(-60);
                    break;
                case KeyAction.Back3Minutes:
                    AddTime(-180);
                    break;
                case KeyAction.Back5Minutes:
                    AddTime(-300);
                    break;
                case KeyAction.Forward15Seconds:
                    AddTime(15);
                    break;
                case KeyAction.Forward30Seconds:
                    AddTime(30);
                    break;
                case KeyAction.Forward1Minute:
                    AddTime(60);
                    break;
                case KeyAction.Forward3Minutes:
                    AddTime(180);
                    break;
                case KeyAction.Forward5Minutes:
                    AddTime(300);
                    break;
                case KeyAction.VolumeUp:
                    AddVolume(5);
                    break;
                case KeyAction.VolumeDown:
                    AddVolume(-5);
                    break;
                case KeyAction.Mute:
                    if (Volume > 0)
                    {
                        _prevvolume = Volume;
                        Volume = 0;
                    }
                    else
                    {
                        Volume = _prevvolume;
                    }
                    break;
                case KeyAction.Volume10Percent:
                    Volume = 10;
                    break;
                case KeyAction.Volume20Percent:
                    Volume = 20;
                    break;
                case KeyAction.Volume30Percent:
                    Volume = 30;
                    break;
                case KeyAction.Volume40Percent:
                    Volume = 40;
                    break;
                case KeyAction.Volume50Percent:
                    Volume = 50;
                    break;
                case KeyAction.Volume60Percent:
                    Volume = 60;
                    break;
                case KeyAction.Volume70Percent:
                    Volume = 70;
                    break;
                case KeyAction.Volume80Percent:
                    Volume = 80;
                    break;
                case KeyAction.Volume90Percent:
                    Volume = 90;
                    break;
                case KeyAction.Volume100Percent:
                    Volume = 100;
                    break;
                case KeyAction.AudioStreams:
                    Bar_AudiosClicked();
                    break;
                case KeyAction.SubtitleStreams:
                    Bar_SubsClicked();
                    break;
                case KeyAction.Settings:
                    Bar_ConfigClicked();
                    break;
                case KeyAction.Fullscreen:
                    Maximize();
                    break;
                case KeyAction.Minimize:
                    Minimize();
                    break;
                case KeyAction.WindowFullscreenToggle:
                    SwitchStates();
                    break;
                case KeyAction.Close:
                    Close();
                    break;
            }
        }
    }
}