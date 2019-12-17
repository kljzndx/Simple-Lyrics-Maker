using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.Media.Core;
using Windows.Media.Playback;
using Windows.UI.Core;
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
        private int _lastSelectedId;

        public TimePointPage()
        {
            this.InitializeComponent();

            Main_MediaPlayerElement.MediaPlayer.PlaybackSession.PositionChanged += PlaybackSession_PositionChanged;
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);

            if (e.Parameter is KeyValuePair<MusicFile, LrcBlock> kvp)
            {
                _lastSelectedId = -1;
                Messenger.Default.Send(kvp, TimePointViewMessageTokens.FileReceived);
                Main_MediaPlayerElement.Source = new MediaPlaybackItem(MediaSource.CreateFromStorageFile(kvp.Key.GetFile()));
            }
        }

        protected override void OnNavigatedFrom(NavigationEventArgs e)
        {
            base.OnNavigatedFrom(e);

            Main_MediaPlayerElement.MediaPlayer.Pause();
        }

        private async void PlaybackSession_PositionChanged(MediaPlaybackSession sender, object args)
        {
            await Dispatcher.RunAsync(CoreDispatcherPriority.Normal,
            () =>
            {
                Messenger.Default.Send(sender.Position, TimePointViewMessageTokens.PositionChanged);
                Main_ScrollSubtitlePreview.Position = sender.Position;
            });
        }

        private void OptionArea_Pivot_OnSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            switch (OptionArea_Pivot.SelectedIndex)
            {
                case 0 when _lastSelectedId != -1:
                    Lyrics_DataGrid.SelectedIndex = _lastSelectedId;
                    break;
                case 1:
                    _lastSelectedId = Lyrics_DataGrid.SelectedIndex;
                    Lyrics_DataGrid.SelectedIndex = -1;
                    break;
            }
        }

        private void Lyrics_DataGrid_OnSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (Lyrics_DataGrid.SelectedIndex > 0)
                Lyrics_DataGrid.ScrollIntoView(Lyrics_DataGrid.SelectedItem, Lyrics_DataGrid.Columns[0]);
        }
    }
}
