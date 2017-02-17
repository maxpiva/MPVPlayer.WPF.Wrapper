using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;

// ReSharper disable RedundantExtendsListEntry

namespace NutzCode.MPVPlayer.WPF.Wrapper.Controls
{
    public partial class Loading : UserControl
    {
        public static readonly DependencyProperty AnimationImageProperty = DependencyProperty.Register("AnimationImage", typeof(ImageSource), typeof(Loading), new FrameworkPropertyMetadata(new BitmapImage(new Uri(@"pack://application:,,,/NutzCode.MPVPlayer.WPF.Wrapper;component/images/spinnerblue.png", UriKind.RelativeOrAbsolute))));
        public static readonly DependencyProperty PercentageProperty = DependencyProperty.Register("Percentage", typeof(string), typeof(Loading), new FrameworkPropertyMetadata("0%"));

        public Loading()
        {
            InitializeComponent();
        }

        public ImageSource AnimationImage
        {
            get { return (ImageSource) GetValue(AnimationImageProperty); }
            set { SetValue(AnimationImageProperty, value); }
        }

        public string Percentage
        {
            get { return (string) GetValue(PercentageProperty); }
            set { SetValue(PercentageProperty, value); }
        }
    }
}