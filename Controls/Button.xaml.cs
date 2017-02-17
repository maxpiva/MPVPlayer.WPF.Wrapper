using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
// ReSharper disable RedundantExtendsListEntry

namespace NutzCode.MPVPlayer.WPF.Wrapper.Controls
{
    public partial class Button : UserControl
    {
        public static readonly DependencyProperty ImageProperty = DependencyProperty.Register("Image", typeof(ImageSource), typeof(Button), new FrameworkPropertyMetadata(null, FrameworkPropertyMetadataOptions.None));
        public static readonly DependencyProperty MouseOverColorProperty = DependencyProperty.Register("MouseOverColor", typeof(Brush), typeof(Button), new FrameworkPropertyMetadata(new SolidColorBrush(Color.FromArgb(0xFF, 0x00, 0x80, 0xd0)), FrameworkPropertyMetadataOptions.Inherits));
        public static readonly DependencyProperty PaddingRelationProperty = DependencyProperty.Register("PaddingRelation", typeof(Size), typeof(Button), new FrameworkPropertyMetadata(new Size(0.125, 0.125), FrameworkPropertyMetadataOptions.AffectsParentMeasure, PaddingRelationChanged));

        static Button()
        {
        }

        public Button()
        {
            InitializeComponent();
            SizeChanged += (a, b) => { CalculatePadding(b.NewSize, PaddingRelation); };
        }

        public ImageSource Image
        {
            get { return (ImageSource) GetValue(ImageProperty); }
            set { SetValue(ImageProperty, value); }
        }

        public Brush MouseOverColor
        {
            get { return (Brush) GetValue(MouseOverColorProperty); }
            set { SetValue(MouseOverColorProperty, value); }
        }

        public Size PaddingRelation
        {
            get { return (Size) GetValue(PaddingRelationProperty); }
            set { SetValue(PaddingRelationProperty, value); }
        }

        public event Delegates.ButtonChangeHandler Clicked;

        protected override void OnMouseUp(MouseButtonEventArgs e)
        {
            Clicked?.Invoke();
        }

        private static void PaddingRelationChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            Button c = (Button) d;
            c.CalculatePadding(new Size(c.Width, c.Height), (Size) e.NewValue);
        }

        private void CalculatePadding(Size s, Size relation)
        {
            double width = s.Width;
            double height = s.Height;
            double wpad = width * relation.Width;
            double hpad = height * relation.Height;
            if (width > height)
                hpad = wpad + (width - height) / 2;
            else if (height > width)
                wpad = hpad + (height - width) / 2;
            Padding = new Thickness(wpad, hpad, wpad, hpad);
        }
    }
}