using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace adrilight.View
{
    public class KeySelectionButton : Button
    {
        public static DependencyProperty ExcludedKeysProperty =
            DependencyProperty.Register(nameof(ExcludedKeys), typeof(int[]), typeof(KeySelectionButton), new PropertyMetadata(new int[0]));

        public int[] ExcludedKeys {
            get => (int[])this.GetValue(ExcludedKeysProperty);
            set => this.SetValue(ExcludedKeysProperty, value);
        }

        public static DependencyProperty SelectedKeyProperty =
            DependencyProperty.Register(nameof(SelectedKey), typeof(int), typeof(KeySelectionButton),
                new FrameworkPropertyMetadata(
                    (a, b) =>
                    {
                        var button = (KeySelectionButton)a;
                        button.SetTextToKey();
                    }));
        public static DependencyProperty SelectedModifiersProperty =
           DependencyProperty.Register(nameof(SelectedModifiers), typeof(List<ModifierKeys>), typeof(KeySelectionButton),
               new FrameworkPropertyMetadata(
                   (a, b) =>
                   {
                       var button = (KeySelectionButton)a;
                       button.SetTextToKey();
                   }));
        private int keycount = 0;
        public int SelectedKey {
            get => (int)this.GetValue(SelectedKeyProperty);
            set => this.SetValue(SelectedKeyProperty, value);
        }
        public List<ModifierKeys> SelectedModifiers {
            get => (List<ModifierKeys>)this.GetValue(SelectedModifiersProperty);
            set => this.SetValue(SelectedModifiersProperty, value);
        }
        private bool _isWaitingForKey;
        private const string ChooseKeyText = "Cài phím tắt";
        private Key _lastKey;
        public KeySelectionButton()
        {
            this.Click += KeySelectButton_Click;
            //this.KeyUp += KeySelectButton_KeyUp;
            this.KeyDown += KeySelectButton_KeyDown;
            this.Content = ChooseKeyText;
        }

        void KeySelectButton_KeyUp(object sender, KeyEventArgs e) // only execut when last key lift up
        {
            keycount--;
            var virtualKeyCode = KeyInterop.VirtualKeyFromKey(e.Key);

            if (this._isWaitingForKey)
            {
                if (this.ExcludedKeys.Contains(virtualKeyCode))
                {
                    return;
                }

                this._isWaitingForKey = false;

                if (e.Key == Key.Escape)
                {
                    this.SetTextToKey();
                    return;
                }
                else
                {

                    this.SelectedKey = virtualKeyCode;
                    if (keycount == 0)
                        this.SetTextToKey();
                }
            }

            
            SelectedModifiers.Clear();
        }
        void KeySelectButton_KeyDown(object sender, KeyEventArgs e)
        {
            //var virtualKeyCode = KeyInterop.VirtualKeyFromKey(e.Key);

            if (e.Key == _lastKey)
                return;
            _lastKey = e.Key;
            var virtualKeyCode = KeyInterop.VirtualKeyFromKey(e.Key);
            keycount++;
            this.SelectedKey = virtualKeyCode;
            //if (this._isWaitingForKey)
            //{
            //    if (this.ExcludedKeys.Contains(virtualKeyCode))
            //    {
            //        return;
            //    }

            //    this._isWaitingForKey = false;

            //    if (e.Key == Key.Escape)
            //    {
            //        this.SetTextToKey();
            //        return;
            //    }
            //    else
            //    {
            //        this.SelectedKey = virtualKeyCode;
            //        this.SetTextToKey();
            //    }
            //}

            //chek if keydown is modifiers
            this.Content = string.Empty;
            if (e.Key == Key.LeftShift || e.Key == Key.RightShift || e.Key == Key.LeftCtrl || e.Key == Key.RightCtrl || e.Key == Key.LeftAlt || e.Key == Key.RightAlt)
            {
                //if holding this modifiers, increment display
                if (SelectedModifiers == null)
                    SelectedModifiers = new List<ModifierKeys>();
                SelectedModifiers.Add((ModifierKeys)e.Key);
                this.SetTextToCombination();

            }
            else
            {
                this.SetTextToKey();
            }


                //    //if (keycount == 3)
                //    //    return;
                //    if (SelectedModifiers == null)
                //        SelectedModifiers = new List<ModifierKeys>();
                //    SelectedModifiers.Add((ModifierKeys)e.Key);
                //}
               

        }

        private void KeySelectButton_Click(object sender, EventArgs e)
        {
            this.Content = ChooseKeyText;
            this._isWaitingForKey = true;
        }

        public void SetTextToKey()
        {

            //if (SelectedModifiers != null)
            //{
            //    foreach (var modifier in this.SelectedModifiers)
            //    {
            //        this.Content += modifier.ToString();
            //        this.Content += " + ";
            //    }
            //}

            this.Content += KeyInterop.KeyFromVirtualKey(this.SelectedKey).ToString();
        }
        public void SetTextToCombination()
        {

            if (SelectedModifiers != null)
            {
                foreach (var modifier in this.SelectedModifiers)
                {
                    this.Content += modifier.ToString();
                    this.Content += " + ";
                }
            }

            this.Content += KeyInterop.KeyFromVirtualKey(this.SelectedKey).ToString();
        }
    }
}