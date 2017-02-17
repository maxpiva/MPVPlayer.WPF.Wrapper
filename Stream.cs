using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using NutzCode.MPVPlayer.WPF.Wrapper.Annotations;
using NutzCode.MPVPlayer.WPF.Wrapper.Models;

namespace NutzCode.MPVPlayer.WPF.Wrapper
{
    public class Stream : INotifyPropertyChanged
    {
        private bool _selected;
        public int Id { get; set; }
        public string Title { get; set; }
        public string Language { get; set; }
        public string ExternalFilename { get; set; }

        public bool Selected
        {
            get { return _selected; }
            set
            {
                _selected = value;
                OnPropertyChanged();
            }
        }

        public string RealTitle => ToString();

        public event PropertyChangedEventHandler PropertyChanged;

        public override string ToString()
        {
            string str = string.Empty;
            if (!string.IsNullOrEmpty(Language))
            {
                if (Language.Length == 3)
                    str += GetLanguageFromCode3(Language, Language);
                else if (Language.Length == 2)
                    str += GetLanguageFromCode2(Language, Language);
                else
                    str += Language;
                if (str.Length > 0)
                    str = str.Substring(0, 1).ToUpperInvariant() + str.Substring(1);
            }
            if (!string.IsNullOrEmpty(Title))
                str += " (" + Title + ")";
            if (!string.IsNullOrEmpty(ExternalFilename))
            {
                string extension = Path.GetExtension(ExternalFilename);
                if (!string.IsNullOrEmpty(extension))
                    str += " [" + extension + "]";
            }
            if (str == string.Empty)
                str = "Unknown";
            return str;
        }

        [NotifyPropertyChangedInvocator]
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        private static string GetLanguageFromCode3(string code3, string full)
        {
            Language l = Models.Language.Languages.FirstOrDefault(a => a.Code3 == code3);
            if (l == null)
                return full;
            return l.Name;
        }

        private static string GetLanguageFromCode2(string code2, string full)
        {
            Language l = Models.Language.Languages.FirstOrDefault(a => a.Code2 == code2);
            if (l == null)
                return full;
            return l.Name;
        }
    }
}