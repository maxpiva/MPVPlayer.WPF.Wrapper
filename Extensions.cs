using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Animation;

namespace NutzCode.MPVPlayer.WPF.Wrapper
{
    public static class Extensions
    {
        public static void VisbilityWithFade(this UIElement v, Visibility visible)
        {
            List<UIElement> c = new List<UIElement>() {v};
            c.VisbilityMembersWithFade(visible);
        }
        public static void VisbilityMembersWithFade(this IEnumerable<UIElement> v, Visibility visible)
        {
            var uiElements = v as IList<UIElement> ?? v.ToList();
            uiElements.VisbilityMembers(Visibility.Visible);
            if (visible==Visibility.Visible)
                return;
            DoubleAnimation danim = new DoubleAnimation
            {
                From = 1.0,
                To = 0.0,
                FillBehavior = FillBehavior.Stop,                
                Duration = new Duration(TimeSpan.FromSeconds(0.5))
            };
            Storyboard s= new Storyboard();
            s.Children.Add(danim);
            foreach (UIElement visual in uiElements)
            {
                visual.Visibility = Visibility.Visible;
            }
            foreach (UIElement c in uiElements)
            {
                Storyboard.SetTarget(danim, c);
            }
            Storyboard.SetTargetProperty(danim, new PropertyPath(UIElement.OpacityProperty));
            s.Completed += delegate { uiElements.VisbilityMembers(visible); };
            s.Begin();
        }

        public static void VisbilityMembers(this IEnumerable<UIElement> v, Visibility visible)
        {
            foreach (UIElement visual in v)
            {
                visual.Visibility = visible;
            }
        }

        public static T GetDefaultValue<T>(this DependencyProperty d)
        {
            return (T) d.DefaultMetadata.DefaultValue;
        }
        public static void GenerateLanguages(this ComboBox box)
        {
            foreach (ComboBoxItem b in Models.Language.Languages.Select(a => a.Name).Distinct().Select(a => new ComboBoxItem { Content = a, Tag = a }))
            {
                box.Items.Add(b);
            }
        }
        public static List<string> GetLanguages(this string setting)
        {
            List<string> ret = new List<string>();
            foreach (string s in setting.Split(',').Select(a=>a.Trim()))
            {
                string n = Models.Language.Languages.FirstOrDefault(a => a.Code3.Equals(s, StringComparison.InvariantCultureIgnoreCase))?.Name;
                if (n != null)
                {
                    if (!ret.Contains(n))
                        ret.Add(n);
                }
            }
            return ret;
        }

        public static ComboBoxItem GetItemByTag(this ComboBox c, string tag)
        {
            foreach (ComboBoxItem cd in c.Items)
            {
                if (((string) cd.Tag) == tag)
                    return cd;
            }
            return null;
        }

    }
}
