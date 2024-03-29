﻿using HandyControl.Data;
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
using System.Windows.Shapes;

namespace adrilight.View
{
    /// <summary>
    /// Interaction logic for PaletteEditWindow.xaml
    /// </summary>
    public partial class ColorPickerWindow 
    {
        public ColorPickerWindow()
        {
            InitializeComponent();
        }
       

        private void ColorPicker_Confirmed(object sender, HandyControl.Data.FunctionEventArgs<Color> e)
        {
            this.Close();
        }

        private void mColorPicker_Canceled(object sender, EventArgs e)
        {
            this.Close();
        }
    }
}
