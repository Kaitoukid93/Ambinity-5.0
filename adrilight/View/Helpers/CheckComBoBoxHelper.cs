using HandyControl.Controls;
using System.Collections.Generic;
using System.Windows;


namespace adrilight.Helpers
{
    public class CheckComBoBoxHelper
    {
        static bool _isUpdating = false;

        public static readonly DependencyProperty SelectedItemsProperty =
            DependencyProperty.RegisterAttached("SelectedItems", typeof(List<string>), typeof(CheckComBoBoxHelper),
                new FrameworkPropertyMetadata(null, new PropertyChangedCallback(OnPropertyChanged)));

        public static List<string> GetSelectedItems(DependencyObject d)
        {
            return d.GetValue(SelectedItemsProperty) as List<string>;
        }

        public static void SetSelectedItems(DependencyObject d, List<string> value)
        {
            d.SetValue(SelectedItemsProperty, value);
        }

        private static void OnPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            CheckComboBox checkComboBox = d as CheckComboBox;

            if (!_isUpdating)
            {
                var result = e.NewValue as List<string>;
                checkComboBox.SelectedItems.Clear();
                if (result != null)
                {
                    foreach (var item in (List<string>)e.NewValue)
                    {
                        checkComboBox.SelectedItems.Add(item);
                    }
                }
                else
                {
                    checkComboBox.SelectedItems.Add(e.NewValue);
                }
            }
        }

        public static readonly DependencyProperty AttachProperty =
         DependencyProperty.RegisterAttached("Attach", typeof(bool), typeof(CheckComBoBoxHelper),
             new FrameworkPropertyMetadata(default(bool), new PropertyChangedCallback(OnAttached)));

        public static bool GetAttach(DependencyObject d)
        {
            return (bool)d.GetValue(AttachProperty);
        }

        public static void SetAttach(DependencyObject d, bool value)
        {
            d.SetValue(AttachProperty, value);
        }

        private static void OnAttached(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            CheckComboBox checkComboBox = d as CheckComboBox;
            checkComboBox.SelectionChanged += CheckComboxSelectionChanged;
        }

        private static void CheckComboxSelectionChanged(object sender, RoutedEventArgs e)
        {
            CheckComboBox checkComboBox = sender as CheckComboBox;
            _isUpdating = true;
            List<string> temp = new List<string>();
            foreach (var item in checkComboBox.SelectedItems)
            {
                temp.Add(item.ToString());
            }
            SetSelectedItems(checkComboBox, temp);
            _isUpdating = false;
        }
    }
}
