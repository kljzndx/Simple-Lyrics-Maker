using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.System;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

//https://go.microsoft.com/fwlink/?LinkId=234236 上介绍了“用户控件”项模板

namespace SimpleLyricsMaker.Views.Controls
{
    public class TextAreaKeyUpEventArgs : EventArgs
    {
        public TextAreaKeyUpEventArgs(bool pressCtrl, bool pressShift, VirtualKey key)
        {
            PressCtrl = pressCtrl;
            PressShift = pressShift;
            Key = key;
        }

        public bool PressCtrl { get; }
        public bool PressShift { get; }
        public VirtualKey Key { get; }
    }

    public sealed partial class TextArea : UserControl
    {
        public static readonly DependencyProperty HeaderProperty = DependencyProperty.Register(
            nameof(Header), typeof(string), typeof(TextArea), new PropertyMetadata(null));

        public static readonly DependencyProperty PlaceholderTextProperty = DependencyProperty.Register(
            nameof(PlaceholderText), typeof(string), typeof(TextArea), new PropertyMetadata(null));

        public event EventHandler<TextAreaKeyUpEventArgs> KeyUpEx;

        private bool _preesCtrl;
        private bool _preesShift;

        public TextArea()
        {
            this.InitializeComponent();

            Root_TextBox.AddHandler(KeyDownEvent, new KeyEventHandler(Root_TextBox_OnKeyDown), true);
            Root_TextBox.AddHandler(KeyUpEvent, new KeyEventHandler(Root_TextBox_OnKeyUp), true);
        }

        public string Header
        {
            get => (string) GetValue(HeaderProperty);
            set => SetValue(HeaderProperty, value);
        }

        public string PlaceholderText
        {
            get => (string) GetValue(PlaceholderTextProperty);
            set => SetValue(PlaceholderTextProperty, value);
        }

        public static readonly DependencyProperty TextProperty = DependencyProperty.Register(
            nameof(Text), typeof(string), typeof(TextArea), new PropertyMetadata(""));

        public string Text
        {
            get => (string) GetValue(TextProperty);
            set => SetValue(TextProperty, value);
        }

        private void Root_TextBox_OnKeyDown(object sender, KeyRoutedEventArgs e)
        {
            switch (e.Key)
            {
                case VirtualKey.Control:
                    _preesCtrl = true;
                    break;
                case VirtualKey.Shift:
                    _preesShift = true;
                    break;
            }
        }

        private void Root_TextBox_OnKeyUp(object sender, KeyRoutedEventArgs e)
        {
            switch (e.Key)
            {
                case VirtualKey.Control:
                    _preesCtrl = false;
                    break;
                case VirtualKey.Shift:
                    _preesShift = false;
                    break;
            }

            KeyUpEx?.Invoke(this, new TextAreaKeyUpEventArgs(_preesCtrl, _preesShift, e.Key));
        }
    }
}
