using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Windows.Storage;
using Windows.Storage.Pickers;
using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Command;
using GalaSoft.MvvmLight.Messaging;
using HappyStudio.Parsing.Subtitle.Interfaces;
using HappyStudio.Parsing.Subtitle.LRC;
using SimpleLyricsMaker.Models;
using SimpleLyricsMaker.ViewModels.Extensions;

namespace SimpleLyricsMaker.ViewModels
{
    public enum TimePointViewMessageTokens
    {
        FileReceived,
        PositionChanged,
        FileSaved
    }

    public class TimePointViewModel : ViewModelBase
    {
        private StorageFolder _saveFolder;
        private MusicFile _musicFile;
        private LrcBlock _lrcBlock;
        private StorageFile _lrcFile;

        private int _selectedId;
        private TimeSpan _currentPosition;

        public TimePointViewModel()
        {
            SetUpTimeCommand = new RelayCommand(SetUpTime, () => SelectedId != -1);
            SaveFileCommand = new RelayCommand(async () => await SaveFile(), () => LrcBlock != null);

            Messenger.Default.Register<SourceInfo>(this, TimePointViewMessageTokens.FileReceived,
            info =>
            {
                _saveFolder = info.Folder;
                MusicFile = info.MusicFile;
                LrcBlock = info.LrcBlock;
                _lrcFile = null;

                SelectedId = info.LrcBlock.Lines.Any() ? 0 : -1;
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
            set
            {
                Set(ref _lrcBlock, value);

                SaveFileCommand.RaiseCanExecuteChanged();
            }
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
        public RelayCommand SaveFileCommand { get; }

        public void SetUpTime()
        {
            LrcBlock.Lines[SelectedId].StartTime = CurrentPosition;
            if (SelectedId < LrcBlock.Lines.Count - 1)
                SelectedId++;
        }

        public async Task SaveFile()
        {
            if (_lrcFile is null)
            {
                if (_saveFolder is null)
                {
                    FileSavePicker picker = new FileSavePicker();
                    picker.FileTypeChoices.Add("LRC file", new List<string>(new []{".lrc"}));
                    picker.SuggestedFileName = MusicFile.FileName.TrimExtensionName();
                    picker.SuggestedStartLocation = PickerLocationId.MusicLibrary;
                    _lrcFile = await picker.PickSaveFileAsync();
                    if (_lrcFile is null)
                        return;
                }
                else
                {
                    _lrcFile = await _saveFolder.CreateFileAsync(MusicFile.FileName.TrimExtensionName() + ".lrc",
                        CreationCollisionOption.FailIfExists);
                }
            }

            await FileIO.WriteTextAsync(_lrcFile, LrcBlock.ToString());
            Messenger.Default.Send(new SourceInfo(_saveFolder, MusicFile, LrcBlock), TimePointViewMessageTokens.FileSaved);
        }
    }
}