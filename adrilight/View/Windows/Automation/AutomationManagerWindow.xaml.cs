﻿using System;
using System.Linq;
using System.Windows;
using System.Windows.Input;

namespace adrilight.View
{
    /// <summary>
    /// Interaction logic for PaletteEditWindow.xaml
    /// </summary>
    public partial class AutomationManagerWindow
    {
        public AutomationManagerWindow()
        {
            InitializeComponent();
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }
        public ModifierKeys[] AllModifiers => Enum.GetValues(typeof(ModifierKeys)).Cast<ModifierKeys>().ToArray();


        protected override void OnContentRendered(EventArgs e)
        {
            base.OnContentRendered(e);


            NonClientAreaContent = new AutomationNonClientAreaContent();

        }



    }
}