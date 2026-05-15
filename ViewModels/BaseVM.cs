using OneColumnEncoder.Models;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace OneColumnEncoder.ViewModels
{
    /// <summary>
    /// Centralized base class for all ViewModels in the application,
    /// providing optional property change notification and resource management.
    /// </summary>
    public class BaseVM : INotifyPropertyChanged
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
        protected virtual void Dispose() { }
        public static void FillCollection(ObservableCollection<ChecklistEntryVM> collection, List<ChecklistItemDefinitionM> definitions)
        {
            collection.Clear();
            foreach (var def in definitions)
            {
                collection.Add(new ChecklistEntryVM
                {
                    Text = def.Text,
                    Status = def.InitialStatus
                });
            }
        }
    }
}
