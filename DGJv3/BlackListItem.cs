using Newtonsoft.Json;
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace DGJv3
{
    class BlackListItem : INotifyPropertyChanged
    {
        [JsonProperty("type")]
        public BlackListType BlackType { get => blackType; set => SetField(ref blackType, value); }

        [JsonProperty("cont")]
        public string Content { get => content; set => SetField(ref content, value); }

        private BlackListType blackType = BlackListType.Id;
        private string content = string.Empty;

        internal BlackListItem()
        { }
        internal BlackListItem(BlackListType type, string content)
        {
            this.BlackType = type;
            this.Content = content;
        }

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
