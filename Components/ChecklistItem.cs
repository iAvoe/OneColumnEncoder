using OneColumnEncoder.Models;
using System.Windows;
using System.Windows.Controls;

namespace OneColumnEncoder.Components
{
    /// <summary>
    /// Provide dependency properties:
    /// - Status icon
    /// - Text to display
    /// 
    /// Usage (for checklist):
    /// - <local:ChecklistItem Status="{Binding CurrentStatus}" Text="This is a status"/>
    /// </summary>
    public class ChecklistItem : Control
    {
        static ChecklistItem()
        {
            DefaultStyleKeyProperty.OverrideMetadata(
                typeof(ChecklistItem),
                new FrameworkPropertyMetadata(typeof(ChecklistItem)));
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
                typeof(ChecklistItem),
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
                typeof(ChecklistItem),
                new PropertyMetadata(string.Empty));
    }
}