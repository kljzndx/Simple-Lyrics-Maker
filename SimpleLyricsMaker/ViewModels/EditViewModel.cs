using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.ServiceModel.Channels;
using System.Threading.Tasks;
using Windows.Storage;
using Windows.Storage.Pickers;
using Windows.Storage.Search;
using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Command;
using GalaSoft.MvvmLight.Messaging;
using HappyStudio.UwpToolsLibrary.Auxiliarys;
using SimpleLyricsMaker.Logs;
using SimpleLyricsMaker.Models;
using SimpleLyricsMaker.ViewModels.Extensions;

namespace SimpleLyricsMaker.ViewModels
{
    public class EditViewModel : ViewModelBase
    {
        private bool _canOpen = true;

        private StorageFolder _folder;

        private List<MusicFile> _allFiles;
        private List<MusicFile> _noLyricFiles;

        private ObservableCollection<MusicFile> _displayFilesList;

        public EditViewModel()
        {
            OpenFolderCommand = new RelayCommand(async () => await OpenFolder(), () => _canOpen);

            Messenger.Default.Register<string>(this, MessageTokens.FolderOpened, async msg => await ScanFile(_folder));
            Messenger.Default.Register<string>(this, MessageTokens.FileScanning, msg => DisplayFilesList = null);
            Messenger.Default.Register<string>(this, MessageTokens.FileScanned, msg => ShowFiles(false));
        }

        public ObservableCollection<MusicFile> DisplayFilesList
        {
            get => _displayFilesList;
            set => Set(ref _displayFilesList, value);
        }

        public RelayCommand OpenFolderCommand { get; }

        public async Task OpenFolder()
        {
            _canOpen = false;
            OpenFolderCommand.RaiseCanExecuteChanged();

            this.LogByObject("正在打开文件夹选取器");
            var picker = new FolderPicker();
            picker.SuggestedStartLocation = PickerLocationId.MusicLibrary;
            picker.FileTypeFilter.Add("*");

            var folder = await picker.PickSingleFolderAsync();
            if (folder != null)
            {
                this.LogByObject("选取成功");
                _folder = folder;
                Messenger.Default.Send(folder.Name, MessageTokens.FolderOpened);
            }

            _canOpen = true;
            OpenFolderCommand.RaiseCanExecuteChanged();
        }

        public async Task ScanFile(StorageFolder folder)
        {
            _allFiles = new List<MusicFile>();
            _noLyricFiles = new List<MusicFile>();

            var lyricsFileNames = new List<string>();

            this.LogByObject("开始扫描文件夹");
            Messenger.Default.Send(folder.Name, MessageTokens.FileScanning);

            QueryOptions queryOptions = new QueryOptions(CommonFileQuery.OrderByName, ".lrc .mp3 .aac .flac .alac .m4a .wav".Split(' '));
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

            Messenger.Default.Send(folder.Name, MessageTokens.FileScanned);
            this.LogByObject("扫描完成");
        }

        public void ShowFiles(bool showAll)
        {
            DisplayFilesList = new ObservableCollection<MusicFile>(showAll ? _allFiles : _noLyricFiles);
        }
    }
}