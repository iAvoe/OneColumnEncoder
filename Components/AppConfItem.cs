using System;
using System.Collections.Generic;
using System.Linq;
using System.Security;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace OneColumnEncoder.Components
{
    public class AppConfItem : ContentControl
    {
        static AppConfItem()
        {
            DefaultStyleKeyProperty.OverrideMetadata(
                typeof(AppConfItem),
                new FrameworkPropertyMetadata(typeof(AppConfItem)));
        }

        // Name of setting, always visible
        public string Text
        {
            get => (string)GetValue(TextProperty);
            set => SetValue(TextProperty, value);
        }
        public static readonly DependencyProperty TextProperty =
            DependencyProperty.Register(
                nameof(Text),
                typeof(string),
                typeof(AppConfItem),
                new PropertyMetadata(string.Empty));

        // The following setting are within ContentPresenters in the template,
        // so they can be hidden when not needed, which is the best practice for WPF.
        /*
        // Checkbox setting value, hide if not needed
        public bool IsChecked
        ...
        // Integer setting value, actually always positive, but WPF doesn't support unsigned types
        public int Number
        ...
        // String setting value, hide if not needed
        public string Text2
        ...
        // For SMTP password (<PasswordBox />), hide if not needed
        public SecureString PasswordHash
        ...
        */
    }
}