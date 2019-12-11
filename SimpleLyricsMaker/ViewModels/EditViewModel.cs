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
using SimpleLyricsMaker.ViewModels.Settings;

namespace SimpleLyricsMaker.ViewModels
{
    public enum EditViewMessageTokens
    {
        FilePicked,
        FolderOpened,
        FileScanning,
        FileScanned,
        FilesSearching,
        FilesSearched,
        FilesShowing,
        FilesShowed,
        SubtitlesTypeChanged,
        FileMade,
    }

    public class EditViewModel : ViewModelBase
    {
        private static readonly string[] allExtensionNames;
        private static readonly FileOpenPicker fileOpenPicker;
        private static readonly FolderPicker folderOpenPicker;

        private bool _canOpen = true;
        private bool _canSearch = true;
        private bool _canSubmit = true;

        private StorageFolder _folder;

        private List<MusicFile> _allFiles;
        private List<MusicFile> _noLyricFiles;

        private MusicFile _currentMusicFile;
        private LrcBlock _currentLyricsFile;

        private string _original = String.Empty;
        private string _translation = String.Empty;

        private ObservableCollection<MusicFile> _displayFilesList;

        static EditViewModel()
        {
            allExtensionNames = new[] {".lrc", ".mp3", ".aac", ".flac", ".alac", ".m4a", ".wav"};

            fileOpenPicker = new FileOpenPicker();
            fileOpenPicker.SuggestedStartLocation = PickerLocationId.MusicLibrary;
            foreach (var name in allExtensionNames.Skip(1))
                fileOpenPicker.FileTypeFilter.Add(name);

            folderOpenPicker = new FolderPicker();
            folderOpenPicker.SuggestedStartLocation = PickerLocationId.MusicLibrary;
            folderOpenPicker.FileTypeFilter.Add("*");
        }

        public EditViewModel()
        {
            base.PropertyChanged += EditViewModel_PropertyChanged;

            OpenFileCommand = new RelayCommand(async () => await OpenFile(), () => _canOpen);
            OpenFolderCommand = new RelayCommand(async () => await OpenFolder(), () => _canOpen);
            RefreshCommand = new RelayCommand(async () => await ScanFile(_folder), () => _allFiles?.Any() ?? false);
            SwitchDisplayCommand = new RelayCommand<bool?>(b => ShowFiles(b ?? false), b => RefreshCommand.CanExecute(null));
            SearchFilesCommand = new RelayCommand<string>(SearchFiles, s => !String.IsNullOrWhiteSpace(s) && _canSearch && RefreshCommand.CanExecute(null));
            SubmitCommand = new RelayCommand(SetUpLyricsContent, () => CurrentLyricsFile != null && _canSubmit);

            Messenger.Default.Register<string>(this, EditViewMessageTokens.FolderOpened, async msg => await ScanFile(_folder));
            Messenger.Default.Register<string>(this, EditViewMessageTokens.FileScanned, msg => ShowFiles(Settings.ShowAll));

            Messenger.Default.Register<ObservableCollection<MusicFile>>(this, EditViewMessageTokens.FilesShowing, list =>
            {
                CurrentMusicFile = null;
                CurrentLyricsFile = null;
            });
            Messenger.Default.Register<ObservableCollection<MusicFile>>(this, EditViewMessageTokens.FilesShowed, list => CurrentMusicFile = DisplayFilesList?.FirstOrDefault());

            Messenger.Default.Register<string>(this, EditViewMessageTokens.FilePicked, msg =>
            {
                Original = String.Empty;
                Translation = String.Empty;
                SubmitCommand.RaiseCanExecuteChanged();
            });

            Messenger.Default.Register<int>(this, EditViewMessageTokens.SubtitlesTypeChanged, id =>
            {
                // 当切换到普通字幕模式时清空“译文”文本框
                // 0 代表的是普通字幕
                if (id == 0)
                    Translation = String.Empty;
            });
        }

        public MusicFile CurrentMusicFile
        {
            get => _currentMusicFile;
            set
            {
                Set(ref _currentMusicFile, value);
                Messenger.Default.Send(value?.FileName, EditViewMessageTokens.FilePicked);
            }
        }

        public LrcBlock CurrentLyricsFile
        {
            get => _currentLyricsFile;
            set => Set(ref _currentLyricsFile, value);
        }

        public ObservableCollection<MusicFile> DisplayFilesList
        {
            get => _displayFilesList;
            set
            {
                Messenger.Default.Send(_displayFilesList, EditViewMessageTokens.FilesShowing);
                Set(ref _displayFilesList, value);
                Messenger.Default.Send(_displayFilesList, EditViewMessageTokens.FilesShowed);
            }
        }

        public string Original
        {
            get => _original;
            set => Set(ref _original, value);
        }

        public string Translation
        {
            get => _translation;
            set => Set(ref _translation, value);
        }

        public EditViewSettingProperties Settings { get; } = EditViewSettingProperties.Current;

        public RelayCommand OpenFileCommand { get; }
        public RelayCommand OpenFolderCommand { get; }
        public RelayCommand RefreshCommand { get; }
        public RelayCommand<bool?> SwitchDisplayCommand { get; }
        public RelayCommand<string> SearchFilesCommand { get; }
        public RelayCommand SubmitCommand { get; }

        public void CreateLyricsFile()
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
            property.MadeBy = Settings.LrcMadeBy;
            property.PropertyChanged += LrcProperties_PropertyChanged;

            if (CurrentLyricsFile != null)
            {
                var oldProperties = (LrcProperties) CurrentLyricsFile.Properties;
                oldProperties.PropertyChanged -= LrcProperties_PropertyChanged;
            }

            CurrentLyricsFile = lrc;
        }

        public void SetUpLyricsContent()
        {
            _canSubmit = false;
            SubmitCommand.RaiseCanExecuteChanged();

            ObservableCollection<LrcLine> lrcLines = (ObservableCollection<LrcLine>) CurrentLyricsFile.Lines;
            string[] originalLines = Original.Trim().ToLines();
            string[] translationLines = Translation.Trim().ToLines();

            bool hasTranslation = translationLines.Any(str => !String.IsNullOrWhiteSpace(str));

            lrcLines.Clear();
            for (var i = 0; i < originalLines.Length; i++)
            {
                string result = originalLines[i];
                if (hasTranslation)
                    result += $" {Settings.SplitSymbol} {translationLines[i < translationLines.Length ? i : translationLines.Length]}";

                lrcLines.Add(new LrcLine(TimeSpan.Zero, result));
            }

            _canSubmit = true;
            SubmitCommand.RaiseCanExecuteChanged();
            Messenger.Default.Send(new KeyValuePair<MusicFile, LrcBlock>(CurrentMusicFile, CurrentLyricsFile), EditViewMessageTokens.FileMade);
        }

        #region Explorer

        public async Task OpenFile()
        {
            _canOpen = false;
            OpenFileCommand.RaiseCanExecuteChanged();
            OpenFolderCommand.RaiseCanExecuteChanged();
            this.LogByObject("正在打开文件夹选取器");

            var file = await fileOpenPicker.PickSingleFileAsync();
            if (file != null)
            {
                this.LogByObject("选取成功");
                var mf = DisplayFilesList?.FirstOrDefault(f => f.FilePath == file.Path);

                if (mf == null)
                {
                    this.LogByObject("正在创建模型");
                    mf = await MusicFile.Create(file);

                    DisplayFilesList?.Insert(0, mf);
                }

                CurrentMusicFile = mf;
            }

            _canOpen = true;
            OpenFileCommand.RaiseCanExecuteChanged();
            OpenFolderCommand.RaiseCanExecuteChanged();
        }

        public async Task OpenFolder()
        {
            _canOpen = false;
            OpenFolderCommand.RaiseCanExecuteChanged();
            OpenFileCommand.RaiseCanExecuteChanged();

            this.LogByObject("正在打开文件夹选取器");
            var folder = await folderOpenPicker.PickSingleFolderAsync();
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
            DisplayFilesList = null;
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
            this.LogByObject($"正在显示文件 showAll = {showAll}");

            DisplayFilesList = new ObservableCollection<MusicFile>(showAll ? _allFiles : _noLyricFiles);
        }

        public void SearchFiles(string fileName)
        {
            _canSearch = false;
            SearchFilesCommand.RaiseCanExecuteChanged();
            string name = fileName.ToLower();
            Messenger.Default.Send(name, EditViewMessageTokens.FilesSearching);
            this.LogByObject("开始搜索");

            DisplayFilesList = new ObservableCollection<MusicFile>(_allFiles.Where(mf => mf.FileName.ToLower().Contains(name)));

            Messenger.Default.Send(name, EditViewMessageTokens.FilesSearched);
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
                        CreateLyricsFile();
                    break;
            }
        }

        private void LrcProperties_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            var theProperties = (LrcProperties)sender;
            switch (e.PropertyName)
            {
                case nameof(theProperties.MadeBy):
                    Settings.LrcMadeBy = theProperties.MadeBy;
                    break;
            }
        }
    }
}