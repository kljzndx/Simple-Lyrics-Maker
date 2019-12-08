using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.ServiceModel.Channels;
using System.Threading.Tasks;
using Windows.Storage;
using Windows.Storage.Pickers;
using Windows.Storage.Search;
using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Command;
using GalaSoft.MvvmLight.Messaging;
using SimpleLyricsMaker.Logs;
using SimpleLyricsMaker.Models;
using SimpleLyricsMaker.ViewModels.Extensions;
using HappyStudio.Parsing.Subtitle.LRC;
using HappyStudio.UwpToolsLibrary.Information;

namespace SimpleLyricsMaker.ViewModels
{
    public enum EditViewMessageTokens
    {
        FilePicked,
        FolderOpened,
        FileScanning,
        FileScanned,
        FilesSeaching,
        FilesSeached,
    }

    public class EditViewModel : ViewModelBase
    {
        private static readonly string[] allExtensionNames;
        private bool _canOpen = true;
        private bool _canSearch = true;

        private StorageFolder _folder;

        private List<MusicFile> _allFiles;
        private List<MusicFile> _noLyricFiles;

        private MusicFile _currentMusicFile;
        private LrcBlock _currentLyricsFile;

        private ObservableCollection<MusicFile> _displayFilesList;

        static EditViewModel()
        {
            allExtensionNames = new[] {".lrc", ".mp3", ".aac", ".flac", ".alac", ".m4a", ".wav"};
        }

        public EditViewModel()
        {
            base.PropertyChanged += EditViewModel_PropertyChanged;

            OpenFileCommand = new RelayCommand(async () => await OpenFile(), () => _canOpen);
            OpenFolderCommand = new RelayCommand(async () => await OpenFolder(), () => _canOpen);
            RefreshCommand = new RelayCommand(async () => await ScanFile(_folder), () => _allFiles?.Any() ?? false);
            SwitchDisplayCommand = new RelayCommand<bool?>(b => ShowFiles(b ?? false), b => RefreshCommand.CanExecute(null));
            SearchFilesCommand = new RelayCommand<string>(SearchFiles, s => !String.IsNullOrWhiteSpace(s) && _canSearch && RefreshCommand.CanExecute(null));

            Messenger.Default.Register<string>(this, EditViewMessageTokens.FolderOpened, async msg => await ScanFile(_folder));
            Messenger.Default.Register<string>(this, EditViewMessageTokens.FileScanning, msg =>
            {
                CurrentMusicFile = null;
                CurrentLyricsFile = null;
                DisplayFilesList = null;
            });
            Messenger.Default.Register<string>(this, EditViewMessageTokens.FileScanned, msg =>
            {
                ShowFiles(false);
                CurrentMusicFile = DisplayFilesList.FirstOrDefault();
            });
        }

        public MusicFile CurrentMusicFile
        {
            get => _currentMusicFile;
            set => Set(ref _currentMusicFile, value);
        }

        public LrcBlock CurrentLyricsFile
        {
            get => _currentLyricsFile;
            set => Set(ref _currentLyricsFile, value);
        }

        public ObservableCollection<MusicFile> DisplayFilesList
        {
            get => _displayFilesList;
            set => Set(ref _displayFilesList, value);
        }

        public RelayCommand OpenFileCommand { get; }
        public RelayCommand OpenFolderCommand { get; }
        public RelayCommand RefreshCommand { get; }
        public RelayCommand<bool?> SwitchDisplayCommand { get; }
        public RelayCommand<string> SearchFilesCommand { get; }

        public LrcBlock CreateLyricsFile()
        {
            var lrc = new LrcBlock();
            var property = (LrcProperties) lrc.Properties;
            if (CurrentMusicFile != null)
            {
                property.Title = CurrentMusicFile.Title;
                property.Artist = CurrentMusicFile.Artist;
                property.Author = CurrentMusicFile.Author;
                property.Album = CurrentMusicFile.Album;
            }

            property.EditorName = AppInfo.Name;
            property.EditorVersion = AppInfo.Version;
            return lrc;
        }

        public async Task OpenFile()
        {
            _canOpen = false;
            OpenFileCommand.RaiseCanExecuteChanged();
            OpenFolderCommand.RaiseCanExecuteChanged();

            var picker = new FileOpenPicker();
            picker.SuggestedStartLocation = PickerLocationId.MusicLibrary;
            foreach (var name in allExtensionNames.Skip(1))
                picker.FileTypeFilter.Add(name);

            var file = await picker.PickSingleFileAsync();
            if (file != null)
            {
                var mf = DisplayFilesList?.FirstOrDefault(f => f.FilePath == file.Path);

                if (mf == null)
                {
                    mf = await MusicFile.Create(file);

                    DisplayFilesList?.Insert(0, mf);
                }

                CurrentMusicFile = mf;

                Messenger.Default.Send(mf.FileName, EditViewMessageTokens.FilePicked);
            }

            _canOpen = true;
            OpenFileCommand.RaiseCanExecuteChanged();
            OpenFolderCommand.RaiseCanExecuteChanged();
        }

        #region Explorer

        public async Task OpenFolder()
        {
            _canOpen = false;
            OpenFolderCommand.RaiseCanExecuteChanged();
            OpenFileCommand.RaiseCanExecuteChanged();

            this.LogByObject("正在打开文件夹选取器");
            var picker = new FolderPicker();
            picker.SuggestedStartLocation = PickerLocationId.MusicLibrary;
            picker.FileTypeFilter.Add("*");

            var folder = await picker.PickSingleFolderAsync();
            if (folder != null)
            {
                this.LogByObject("选取成功");
                _folder = folder;
                Messenger.Default.Send(folder.Name, EditViewMessageTokens.FolderOpened);
            }

            _canOpen = true;
            OpenFolderCommand.RaiseCanExecuteChanged();
            OpenFileCommand.RaiseCanExecuteChanged();
        }

        public async Task ScanFile(StorageFolder folder)
        {
            _allFiles = new List<MusicFile>();
            _noLyricFiles = new List<MusicFile>();

            var lyricsFileNames = new List<string>();

            RefreshCommand.RaiseCanExecuteChanged();
            SwitchDisplayCommand.RaiseCanExecuteChanged();
            SearchFilesCommand.RaiseCanExecuteChanged();
            Messenger.Default.Send(folder.Name, EditViewMessageTokens.FileScanning);
            this.LogByObject("开始扫描文件夹");

            QueryOptions queryOptions = new QueryOptions(CommonFileQuery.OrderByName, allExtensionNames);
            queryOptions.FolderDepth = FolderDepth.Shallow;
            var queryResult = folder.CreateFileQueryWithOptions(queryOptions);

            for (uint i = 0; i < await queryResult.GetItemCountAsync(); i += 50)
            {
                var files = await queryResult.GetFilesAsync(i, 50);
                foreach (var file in files)
                {
                    if (file.FileType.ToLower() == ".lrc")
                        lyricsFileNames.Add(file.Name);
                    else
                        _allFiles.Add(await MusicFile.Create(file));
                }
            }

            this.LogByObject("筛选没有相应歌词文件的音乐文件");
            _noLyricFiles.AddRange(_allFiles.Where(mf => lyricsFileNames.All(lfn => lfn.TrimExtensionName() != mf.FileName.TrimExtensionName())));

            RefreshCommand.RaiseCanExecuteChanged();
            SwitchDisplayCommand.RaiseCanExecuteChanged();
            SearchFilesCommand.RaiseCanExecuteChanged();
            Messenger.Default.Send(folder.Name, EditViewMessageTokens.FileScanned);
            this.LogByObject("扫描完成");
        }

        public void ShowFiles(bool showAll)
        {
            DisplayFilesList = new ObservableCollection<MusicFile>(showAll ? _allFiles : _noLyricFiles);
        }

        public void SearchFiles(string fileName)
        {
            _canSearch = false;
            SearchFilesCommand.RaiseCanExecuteChanged();
            string name = fileName.ToLower();
            Messenger.Default.Send(name, EditViewMessageTokens.FilesSeaching);

            DisplayFilesList = new ObservableCollection<MusicFile>(_allFiles.Where(mf => mf.FileName.ToLower().Contains(name)));

            Messenger.Default.Send(name, EditViewMessageTokens.FilesSeached);
            _canSearch = true;
            SearchFilesCommand.RaiseCanExecuteChanged();
        }

        #endregion

        private void EditViewModel_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            switch (e.PropertyName)
            {
                case nameof(CurrentMusicFile):
                    if (CurrentMusicFile != null)
                        CurrentLyricsFile = CreateLyricsFile();
                    break;
            }
        }
    }
}