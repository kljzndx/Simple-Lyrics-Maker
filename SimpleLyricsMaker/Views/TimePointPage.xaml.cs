using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.Media.Core;
using Windows.Media.Playback;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;
using GalaSoft.MvvmLight.Messaging;
using HappyStudio.Parsing.Subtitle.LRC;
using SimpleLyricsMaker.Models;
using SimpleLyricsMaker.ViewModels;

// https://go.microsoft.com/fwlink/?LinkId=234238 上介绍了“空白页”项模板

namespace SimpleLyricsMaker.Views
{
    /// <summary>
    /// 可用于自身或导航至 Frame 内部的空白页。
    /// </summary>
    public sealed partial class TimePointPage : Page
    {
        public TimePointPage()
        {
            this.InitializeComponent();
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);

            if (e.Parameter is KeyValuePair<MusicFile, LrcBlock> kvp)
            {
                Messenger.Default.Send(kvp, TimePointViewMessageTokens.FileReceived);
                Main_MediaPlayerElement.Source = new MediaPlaybackItem(MediaSource.CreateFromStorageFile(kvp.Key.GetFile()));
            }
        }

        protected override void OnNavigatedFrom(NavigationEventArgs e)
        {
            base.OnNavigatedFrom(e);

            Main_MediaPlayerElement.MediaPlayer.Pause();
        }
    }
}
