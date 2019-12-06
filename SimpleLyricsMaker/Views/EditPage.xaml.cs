using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.ApplicationModel.Resources;
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
        private static readonly string FileScanningText1 = ResourceLoader.GetForCurrentView("EditPage").GetString("LoadingInfo_FileScanning_Text1");
        private static readonly string FileScanningText3 = ResourceLoader.GetForCurrentView("EditPage").GetString("LoadingInfo_FileScanning_Text3");

        private static readonly string FileSearchingText1 = ResourceLoader.GetForCurrentView("EditPage").GetString("LoadingInfo_FileSearching_Text1");

        public EditPage()
        {
            this.InitializeComponent();

            Messenger.Default.Register<string>(this, EditViewMessageTokens.FileScanning, msg =>
            {
                Root_SplitView.IsPaneOpen = true;
                ShowLoading(FileScanningText1, msg, FileScanningText3);
            });
            Messenger.Default.Register<string>(this, EditViewMessageTokens.FileScanned, msg => HideLoading());

            Messenger.Default.Register<string>(this, EditViewMessageTokens.FilesSeaching, msg => ShowLoading(FileSearchingText1, msg, String.Empty));
            Messenger.Default.Register<string>(this, EditViewMessageTokens.FilesSeached, msg => HideLoading());
        }

        private void ShowLoading(string left, string center, string right)
        {
            Loading_StackPanel.Visibility = Visibility.Visible;
            Loading_ProgressRing.IsActive = true;
            LoadingInfo_TextBlock.Text = String.Concat(left, center, right);
        }

        private void HideLoading()
        {
            LoadingInfo_TextBlock.Text = String.Empty;
            Loading_ProgressRing.IsActive = false;
            Loading_StackPanel.Visibility = Visibility.Collapsed;
        }
    }
}
