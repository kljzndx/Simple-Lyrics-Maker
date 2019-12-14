using System;
using System.Collections.Generic;
using System.Linq;
using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Command;
using GalaSoft.MvvmLight.Messaging;
using HappyStudio.Parsing.Subtitle.LRC;
using SimpleLyricsMaker.Models;

namespace SimpleLyricsMaker.ViewModels
{
    public enum TimePointViewMessageTokens
    {
        FileReceived,
        PositionChanged
    }

    public class TimePointViewModel : ViewModelBase
    {
        private MusicFile _musicFile;
        private LrcBlock _lrcBlock;

        private LrcLine _selectedLine;
        private TimeSpan _currentPosition;

        public TimePointViewModel()
        {
            SetUpTimeCommand = new RelayCommand(SetUpTime, () => SelectedLine != null);

            Messenger.Default.Register<KeyValuePair<MusicFile, LrcBlock>>(this, TimePointViewMessageTokens.FileReceived,
            kvp =>
            {
                MusicFile = kvp.Key;
                LrcBlock = kvp.Value;
                SelectedLine = LrcBlock.Lines.FirstOrDefault() as LrcLine;
            });

            Messenger.Default.Register<TimeSpan>(this, TimePointViewMessageTokens.PositionChanged, p => CurrentPosition = p);
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

        public LrcLine SelectedLine
        {
            get => _selectedLine;
            set
            {
                Set(ref _selectedLine, value);

                SetUpTimeCommand.RaiseCanExecuteChanged();
            }
        }

        public TimeSpan CurrentPosition
        {
            get => _currentPosition;
            set => Set(ref _currentPosition, value);
        }

        public RelayCommand SetUpTimeCommand { get; }

        public void SetUpTime()
        {
            SelectedLine.StartTime = CurrentPosition;
        }
    }
}