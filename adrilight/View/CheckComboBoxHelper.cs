using HandyControl.Controls;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using NonInvasiveKeyboardHookLibrary;


namespace adrilight.View
{
    public class CheckComboBoxHelper
    {
        static bool _isUpdating = false;

        public static readonly DependencyProperty SelectedItemsProperty =
            DependencyProperty.RegisterAttached("SelectedItems", typeof(List<IModifiersType>), typeof(CheckComboBoxHelper),
                new FrameworkPropertyMetadata(null, new PropertyChangedCallback(OnPropertyChanged)));

        public static List<IModifiersType> GetSelectedItems(DependencyObject d)
        {
            return d.GetValue(SelectedItemsProperty) as List<IModifiersType>;
        }

        public static void SetSelectedItems(DependencyObject d, List<IModifiersType> value)
        {
            d.SetValue(SelectedItemsProperty, value);
        }

        private static void OnPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            CheckComboBox checkComboBox = d as CheckComboBox;

            if (!_isUpdating)
            {
                var result = e.NewValue as List<IModifiersType>;
                checkComboBox.SelectedItems.Clear();
                if (result != null)
                {
                    foreach (var item in (List<IModifiersType>)e.NewValue)
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
         DependencyProperty.RegisterAttached("Attach", typeof(bool), typeof(CheckComboBoxHelper),
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
            List<IModifiersType> temp = new List<IModifiersType>();
            foreach (IModifiersType item in checkComboBox.SelectedItems)
            {
                temp.Add(item);
            }
            SetSelectedItems(checkComboBox, temp);
            _isUpdating = false;
        }
    }
}
