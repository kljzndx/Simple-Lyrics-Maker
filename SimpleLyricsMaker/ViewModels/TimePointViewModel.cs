using System.Collections.Generic;
using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Messaging;
using HappyStudio.Parsing.Subtitle.LRC;
using SimpleLyricsMaker.Models;

namespace SimpleLyricsMaker.ViewModels
{
    public enum TimePointViewMessageTokens
    {
        FileReceived
    }

    public class TimePointViewModel : ViewModelBase
    {
        private MusicFile _musicFile;

        private LrcBlock _lrcBlock;

        public TimePointViewModel()
        {
            Messenger.Default.Register<KeyValuePair<MusicFile, LrcBlock>>(this, TimePointViewMessageTokens.FileReceived,
            kvp =>
            {
                MusicFile = kvp.Key;
                LrcBlock = kvp.Value;
            });
        }

        public MusicFile MusicFile
        {
            get => _musicFile;
            set => Set(ref _musicFile, value);
        }

        public LrcBlock LrcBlock
        {
            get => _lrcBlock;
            set => Set(ref _lrcBlock, value);
        }
    }
}