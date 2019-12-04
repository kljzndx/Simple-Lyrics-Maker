using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;
using GalaSoft.MvvmLight.Messaging;
using SimpleLyricsMaker.ViewModels;

// https://go.microsoft.com/fwlink/?LinkId=234238 上介绍了“空白页”项模板

namespace SimpleLyricsMaker.Views
{
    /// <summary>
    /// 可用于自身或导航至 Frame 内部的空白页。
    /// </summary>
    public sealed partial class EditPage : Page
    {
        public EditPage()
        {
            this.InitializeComponent();

            Messenger.Default.Register<string>(this, MessageTokens.FileScanning, msg =>
            {
                Root_SplitView.IsPaneOpen = true;
                FileScanning_StackPanel.Visibility = Visibility.Visible;
                FileScanning_ProgressRing.IsActive = true;
                Folder_Run.Text = msg;
            });
            Messenger.Default.Register<string>(this, MessageTokens.FileScanned, msg =>
            {
                FileScanning_ProgressRing.IsActive = false;
                FileScanning_StackPanel.Visibility = Visibility.Collapsed;
            });
        }
    }
}
