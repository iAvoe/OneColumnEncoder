using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace OneColumnEncoder.ViewModels
{
    /// <summary>
    /// Centralized base class for all ViewModels in the application,
    /// providing optional property change notification and resource management.
    /// </summary>
    public class BaseVM : INotifyPropertyChanged, IDisposable
    {
        public event PropertyChangedEventHandler? PropertyChanged;
        protected virtual void OnPropertyChanged([CallerMemberName] string? propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        protected bool SetProperty<T>(ref T field, T value, [CallerMemberName] string? propertyName = null)
        {
            if (EqualityComparer<T>.Default.Equals(field, value)) return false;
            field = value;
            OnPropertyChanged(propertyName);
            return true;
        }
        public virtual void Dispose() { }
        public static void FillCollection(ObservableCollection<ChecklistEntryVM> collection, List<ChecklistItemDefinitionM> definitions)
        {
            collection.Clear();
            foreach (ChecklistItemDefinitionM d in definitions)
            {
                collection.Add(new ChecklistEntryVM
                {
                    Text = d.Text,
                    Status = d.InitialStatus
                });
            }
        }
    }
}
