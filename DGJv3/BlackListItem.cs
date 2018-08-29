using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace DGJv3
{
    class BlackListItem : INotifyPropertyChanged
    {
        public BlackListType BlackType { get => blackType; set => SetField(ref blackType, value); }
        public string Content { get => content; set => SetField(ref content, value); }

        private BlackListType blackType;
        private string content;

        public event PropertyChangedEventHandler PropertyChanged;
        protected bool SetField<T>(ref T field, T value, [CallerMemberName] string propertyName = "")
        {
            if (EqualityComparer<T>.Default.Equals(field, value)) return false;
            field = value;
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
            return true;
        }
    }
}
