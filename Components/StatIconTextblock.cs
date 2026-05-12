using System;
using System.Collections.Generic;
using System.Linq;
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
using OneColumnEncoder.CommonMethods;

namespace OneColumnEncoder.Components
{
    /// <summary>
    /// Provide dependency properties:
    /// - Status for implict understanding
    /// - Text to display
    /// 
    /// Usage:
    /// - <local:StatIconTextblock Status="{Binding CurrentStatus}" Text="This is a status"/>
    /// </summary>
    public class StatIconTextblock : Control
    {
        static StatIconTextblock()
        {
            DefaultStyleKeyProperty.OverrideMetadata(
                typeof(StatIconTextblock),
                new FrameworkPropertyMetadata(typeof(StatIconTextblock))
            );
        }

        public StatusType Status
        {
            get => (StatusType)GetValue(StatusProperty);
            set => SetValue(StatusProperty, value);
        }
        public static readonly DependencyProperty StatusProperty =
            DependencyProperty.Register(
                nameof(Status),
                typeof(StatusType),
                typeof(StatIconTextblock),
                new PropertyMetadata(StatusType.Waiting));
        
        public string Text
        {
            get => (string)GetValue(TextProperty);
            set => SetValue(TextProperty, value);
        }
        public static readonly DependencyProperty TextProperty =
            DependencyProperty.Register(
                nameof(Text),
                typeof(string),
                typeof(StatIconTextblock),
                new PropertyMetadata(string.Empty));
    }
}
