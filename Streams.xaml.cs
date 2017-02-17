using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
// ReSharper disable RedundantExtendsListEntry

namespace NutzCode.MPVPlayer.WPF.Wrapper
{
    /// <summary>
    /// Interaction logic for Streams.xaml
    /// </summary>
    public partial class Streams : Window
    {
        public delegate void StreamChangedHandler(Stream s);

        private readonly bool _canbeunselected;

        public Streams(string title, bool canbeunselected)
        {
            _canbeunselected = canbeunselected;
            InitializeComponent();
            Title = title;
            MouseDown += Streams_MouseDown;
        }


        public event StreamChangedHandler StreamChanged;

        private void Streams_MouseDown(object sender, MouseButtonEventArgs e)
        {
            DragMove();
        }

        private void ToggleButton_OnChecked(object sender, RoutedEventArgs e)
        {
            CheckBox chk = (CheckBox) sender;
            Stream str = (Stream) chk.DataContext;
            ObservableCollection<Stream> s = (ObservableCollection<Stream>) DataContext;
            if (!str.Selected)
            {
                if (_canbeunselected)
                {
                    StreamChanged?.Invoke(null);
                    return;
                }
                str.Selected = true;
                e.Handled = true;
            }
            else
            {
                Stream n = s.FirstOrDefault(a => a.Selected && a != str);
                if (n != null)
                    n.Selected = false;
                StreamChanged?.Invoke(str);
            }
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

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            Window w = Owner;
            Left = w.Left + w.Width - ActualWidth;
            Top = w.Top + w.Height - ActualHeight - 48;
        }
    }
}