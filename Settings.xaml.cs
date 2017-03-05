using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using NutzCode.MPVPlayer.WPF.Wrapper.Models;

// ReSharper disable RedundantExtendsListEntry

namespace NutzCode.MPVPlayer.WPF.Wrapper
{
    /// <summary>
    /// Interaction logic for Streams.xaml
    /// </summary>
    public partial class Settings : Window
    {
        private readonly List<Device> _devices;

        private readonly PlayerSettings _settings;

        private readonly List<Codec> _codecList = new List<Codec>()
        {
            new Codec {Name = "Dolby Digital", Tag = "ac3"},
            new Codec {Name = "DTS", Tag = "dts"},
            new Codec {Name = "Dolby Digital Plus", Tag = "eac3"},
            new Codec {Name = "DTS-HD Master", Tag = "dtshd"},
            new Codec {Name = "Dolby TrueHD", Tag = "truehd"}
        };

        public ObservableCollection<Codec> Codecs = new ObservableCollection<Codec>();

        public Settings(PlayerSettings settings, List<Device> devices)
        {
            _settings = new PlayerSettings
            {
                AudioOutput = settings.AudioOutput,
                Passthrough = settings.Passthrough,
                ExclusiveMode = settings.ExclusiveMode,
                NormalizeVolume = settings.NormalizeVolume,
                HardwareDecoding = settings.HardwareDecoding,
                PreferredSubtitles = settings.PreferredSubtitles,
                SubtitleSize = settings.SubtitleSize,
                SubtitlesEnabled = settings.SubtitlesEnabled,
                Device = settings.Device,
                PreferredAudios = settings.PreferredAudios,
                Channels = settings.Channels
            };
            _devices = devices;
            InitializeComponent();
            InitSettings();
            MouseDown += Streams_MouseDown;
        }

        public event Delegates.SettingsChangedHandler SettingsChanged;

        private void InitSettings()
        {
            AudioOutput.SelectedItem = AudioOutput.GetItemByTag(_settings.AudioOutput);
            Channels.SelectedItem = Channels.GetItemByTag(_settings.Channels);
            foreach (Device d in _devices)
                Device.Items.Add(new ComboBoxItem {Content = d.Description, Tag = d.Name});
            ComboBoxItem b = Device.GetItemByTag(_settings.Device);
            Device.SelectedItem = b ?? Device.Items[0];
            Exclusive.IsChecked = _settings.ExclusiveMode;
            Normalize.IsChecked = _settings.NormalizeVolume;
            SubsEnabled.IsChecked = _settings.SubtitlesEnabled;
            HwdDecoding.IsChecked = _settings.HardwareDecoding;
            SubsPref1.GenerateLanguages();
            SubsPref1.GenerateLanguages();
            SubsPref2.GenerateLanguages();
            LanguagePref1.GenerateLanguages();
            LanguagePref2.GenerateLanguages();
            List<string> langs = _settings.PreferredAudios.Split(',').Select(a => a.Trim()).ToList();
            if (langs.Count > 0)
                LanguagePref1.SelectedItem = LanguagePref1.GetItemByTag(langs[0]);
            if (langs.Count > 1)
                LanguagePref2.SelectedItem = LanguagePref2.GetItemByTag(langs[1]);
            langs = _settings.PreferredSubtitles.Split(',').Select(a => a.Trim()).ToList();
            if (langs.Count > 0)
                SubsPref1.SelectedItem = SubsPref1.GetItemByTag(langs[0]);
            if (langs.Count > 1)
                SubsPref2.SelectedItem = SubsPref2.GetItemByTag(langs[1]);
            Size.SelectedItem = Size.GetItemByTag(_settings.SubtitleSize.ToString(CultureInfo.InvariantCulture));
            GeneratePassthroughs();
        }

        private void UpdatePassthrough(string option, bool state)
        {
            List<string> passes = _settings.Passthrough.Split(',').Select(a => a.Trim().ToLowerInvariant()).ToList();
            if (passes.Contains(option))
                passes.Remove(option);
            if (state)
                passes.Add(option);
            _settings.Passthrough = string.Join(",", passes);
            SettingsChanged?.Invoke(_settings);
        }

        public void GeneratePassthroughs()
        {
            List<string> passes = _settings.Passthrough.Split(',').Select(a => a.Trim().ToLowerInvariant()).ToList();
            Passthrough.ItemsSource = Codecs;

            foreach (Codec c in _codecList)
            {
                c.IsChecked = passes.Contains(c.Tag);
                c.CheckChanged += () => UpdatePassthrough(c.Tag, c.IsChecked);
            }


            if (_settings.AudioOutput == "auto")
            {
                Passthrough.Visibility = Visibility.Collapsed;
                LabelPassthrough.Visibility = Visibility.Collapsed;
            }
            else
            {
                Passthrough.Visibility = Visibility.Visible;
                LabelPassthrough.Visibility = Visibility.Visible;
            }
            if (_settings.AudioOutput == "Optical")
            {
                Codecs.Clear();
                Codecs.Add(_codecList.FirstOrDefault(a => a.Tag == "ac3"));
                Codecs.Add(_codecList.FirstOrDefault(a => a.Tag == "dts"));
            }
            else if (_settings.AudioOutput == "HDMI")
            {
                Codecs.Clear();
                foreach (Codec c in _codecList)
                    Codecs.Add(c);
            }
        }

        private void Streams_MouseDown(object sender, MouseButtonEventArgs e)
        {
            DragMove();
        }


        private void Button_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            e.Cancel = true;
            Visibility = Visibility.Hidden;
        }

        private void HwdDecoding_Checked(object sender, RoutedEventArgs e)
        {
            // ReSharper disable once PossibleInvalidOperationException
            _settings.HardwareDecoding = HwdDecoding.IsChecked.Value;
            SettingsChanged?.Invoke(_settings);
        }

        private void SubsEnabled_Checked(object sender, RoutedEventArgs e)
        {
            // ReSharper disable once PossibleInvalidOperationException
            _settings.SubtitlesEnabled = SubsEnabled.IsChecked.Value;
            SettingsChanged?.Invoke(_settings);
        }

        private void Size_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            ComboBoxItem b = Size.SelectedItem as ComboBoxItem;
            if (b != null)
                _settings.SubtitleSize = double.Parse((string) b.Tag);
            SettingsChanged?.Invoke(_settings);
        }

        private void SubsPref1_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            string str = string.Empty;
            ComboBoxItem b = SubsPref1.SelectedItem as ComboBoxItem;
            if (b != null)
                str += (string) b.Tag;
            b = SubsPref2.SelectedItem as ComboBoxItem;
            if (b != null)
            {
                if (!string.IsNullOrEmpty(str))
                    str += ",";
                str += (string) b.Tag;
            }
            _settings.PreferredSubtitles = str;
            SettingsChanged?.Invoke(_settings);
        }

        private void AudioOutput_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            ComboBoxItem b = AudioOutput.SelectedItem as ComboBoxItem;
            if (b != null)
                _settings.AudioOutput = (string) b.Tag;
            GeneratePassthroughs();
            SettingsChanged?.Invoke(_settings);
        }

        private void Channels_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            ComboBoxItem b = Channels.SelectedItem as ComboBoxItem;
            if (b != null)
                _settings.Channels = (string) b.Tag;
            SettingsChanged?.Invoke(_settings);
        }

        private void Device_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            ComboBoxItem b = Device.SelectedItem as ComboBoxItem;
            if (b != null)
                _settings.Device = (string) b.Tag;
            SettingsChanged?.Invoke(_settings);
        }

        private void Normalize_Checked(object sender, RoutedEventArgs e)
        {
            // ReSharper disable once PossibleInvalidOperationException
            _settings.NormalizeVolume = Normalize.IsChecked.Value;
            SettingsChanged?.Invoke(_settings);
        }

        private void Exclusive_Checked(object sender, RoutedEventArgs e)
        {
            // ReSharper disable once PossibleInvalidOperationException
            _settings.ExclusiveMode = Exclusive.IsChecked.Value;
            SettingsChanged?.Invoke(_settings);
        }

        private void LanguagePref1_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            string str = string.Empty;
            ComboBoxItem b = LanguagePref1.SelectedItem as ComboBoxItem;
            if (b != null)
                str += (string) b.Tag;
            b = LanguagePref2.SelectedItem as ComboBoxItem;
            if (b != null)
            {
                if (!string.IsNullOrEmpty(str))
                    str += ",";
                str += (string) b.Tag;
            }
            _settings.PreferredAudios = str;
            SettingsChanged?.Invoke(_settings);
        }

        public class Codec
        {
            public delegate void CheckedChangedHandler();

            private bool _isChecked;
            public string Name { get; set; }

            public string Tag { get; set; }

            public bool IsChecked
            {
                get { return _isChecked; }
                set
                {
                    _isChecked = value;
                    CheckChanged?.Invoke();
                }
            }

            public event CheckedChangedHandler CheckChanged;
        }
    }
}