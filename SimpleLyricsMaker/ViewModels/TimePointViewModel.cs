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
        private bool _canDelay = true;

        private StorageFolder _saveFolder;
        private MusicFile _musicFile;
        private LrcBlock _lrcBlock;
        private StorageFile _lrcFile;

        private int _selectedId;
        private TimeSpan _currentPosition;

        public TimePointViewModel()
        {
            AddLineCommand = new RelayCommand(AddLine);
            RemoveLineCommand = new RelayCommand<ISubtitleLine>(l => LrcBlock.Lines.Remove(l), l => l != null);
            SetUpTimeCommand = new RelayCommand(SetUpTime, () => SelectedId != -1);
            DelayAllCommand = new RelayCommand<double>(DelayAll, ms => _canDelay);
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

        public RelayCommand AddLineCommand { get; }
        public RelayCommand<ISubtitleLine> RemoveLineCommand { get; }
        public RelayCommand SetUpTimeCommand { get; }
        public RelayCommand<double> DelayAllCommand { get; }
        public RelayCommand SaveFileCommand { get; }

        public void AddLine()
        {
            LrcBlock.Lines.Insert(SelectedId < 0 ? LrcBlock.Lines.Count : SelectedId + 1, new LrcLine(CurrentPosition));
        }

        public void SetUpTime()
        {
            LrcBlock.Lines[SelectedId].StartTime = CurrentPosition;
            if (SelectedId < LrcBlock.Lines.Count - 1)
                SelectedId++;
        }

        public void DelayAll(double ms)
        {
            _canDelay = false;
            DelayAllCommand.RaiseCanExecuteChanged();

            bool isAdd = ms > 0;

            TimeSpan msTime = TimeSpan.FromMilliseconds(isAdd ? ms : -ms);
            foreach (var line in LrcBlock.Lines)
                if (isAdd)
                    line.StartTime += msTime;
                else
                    line.StartTime -= msTime;

            _canDelay = true;
            DelayAllCommand.RaiseCanExecuteChanged();
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