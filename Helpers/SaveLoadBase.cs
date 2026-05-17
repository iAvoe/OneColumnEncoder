using System;
using System.ComponentModel;
using System.IO;
using System.Text.Json;

namespace OneColumnEncoder.Helpers
{
    public abstract class SaveLoadBase<T> : INotifyPropertyChanged where T : SaveLoadBase<T>, new()
    {
        private static readonly JsonSerializerOptions CachedJsonOptions =
            new() { WriteIndented = true };

        public event PropertyChangedEventHandler? PropertyChanged;

        protected abstract string FilePath { get; }

        protected virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        public void Save()
        {
            try
            {
                var json =
                    JsonSerializer.Serialize((T)this, CachedJsonOptions);
                File.WriteAllText(FilePath, json);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error saving: {ex.Message}");
            }
        }

        public static T Load()
        {
            var instance = new T();
            if (!File.Exists(instance.FilePath)) return instance;
            try
            {
                var json = File.ReadAllText(instance.FilePath);
                return JsonSerializer.Deserialize<T>(json) ?? instance;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error loading: {ex.Message}");
            }
            return instance;
        }
    }
}
