using System;
using System.Collections.Generic;
using System.Linq;
using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Command;
using GalaSoft.MvvmLight.Messaging;
using HappyStudio.Parsing.Subtitle.Interfaces;
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

        private int _selectedId;
        private TimeSpan _currentPosition;

        public TimePointViewModel()
        {
            SetUpTimeCommand = new RelayCommand(SetUpTime, () => SelectedId != -1);

            Messenger.Default.Register<SourceInfo>(this, TimePointViewMessageTokens.FileReceived,
            info =>
            {
                MusicFile = info.MusicFile;
                LrcBlock = info.LrcBlock;
                SelectedId = 0;
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

        public int SelectedId
        {
            get => _selectedId;
            set
            {
                Set(ref _selectedId, value);

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
            LrcBlock.Lines[SelectedId].StartTime = CurrentPosition;
            if (SelectedId < LrcBlock.Lines.Count - 1)
                SelectedId++;
        }
    }
}