using System;
using System.Threading.Tasks;
using Windows.Storage;
using Windows.Storage.FileProperties;
using Windows.UI.Xaml.Media.Imaging;
using SimpleLyricsMaker.ViewModels.Extensions;

namespace SimpleLyricsMaker.Models
{
    public class MusicFile
    {
        private readonly WeakReference<StorageFile> _file = new WeakReference<StorageFile>(null);
        private readonly WeakReference<StorageItemThumbnail> _albumCoverSource = new WeakReference<StorageItemThumbnail>(null);
        private readonly WeakReference<BitmapImage> _albumCover = new WeakReference<BitmapImage>(null);

        private MusicFile(StorageFile file, MusicProperties musicProperties)
        {
            Title = musicProperties.Title;
            Artist = musicProperties.Artist;
            Album = musicProperties.Album;

            FileName = file.Name;
            FilePath = file.Path;
        }

        public string Title { get; }
        public string Artist { get; }
        public string Album { get; }

        public string FileName { get; }
        public string FilePath { get; }

        public async Task<StorageFile> GetFile()
        {
            return await _file.GetTarget(async () => await StorageFile.GetFileFromPathAsync(FilePath));
        }

        public async Task<StorageItemThumbnail> GetAlbumCoverSource()
        {
            return await _albumCoverSource.GetTarget(async () =>
            {
                var file = await GetFile();
                return await file.GetThumbnailAsync(ThumbnailMode.MusicView);
            });
        }

        public async Task<BitmapImage> GetAlbumCover()
        {
            return await _albumCover.GetTarget(async () =>
            {
                var cover = await GetAlbumCoverSource();
                var image = new BitmapImage();
                image.SetSource(cover);
                return image;
            });
        }

        public static async Task<MusicFile> Create(StorageFile file)
        {
            return new MusicFile(file, await file.Properties.GetMusicPropertiesAsync());
        }
    }
}