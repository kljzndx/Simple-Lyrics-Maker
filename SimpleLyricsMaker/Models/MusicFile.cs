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
        private readonly StorageFile _file;
        private readonly WeakReference<StorageItemThumbnail> _albumCoverSource = new WeakReference<StorageItemThumbnail>(null);
        private readonly WeakReference<BitmapImage> _albumCover = new WeakReference<BitmapImage>(null);

        private MusicFile(StorageFile file, MusicProperties musicProperties)
        {
            _file = file;
            FileName = file.Name;
            FilePath = file.Path;

            Title = musicProperties.Title;
            Artist = musicProperties.Artist;
            Author = String.Join(", ", musicProperties.Writers);
            Album = musicProperties.Album;
        }

        public string Title { get; }
        public string Artist { get; }
        public string Author { get; }
        public string Album { get; }

        public string FileName { get; }
        public string FilePath { get; }

        public StorageFile GetFile()
        {
            return _file;
        }

        public async Task<StorageItemThumbnail> GetAlbumCoverSource()
        {
            return await _albumCoverSource.GetTarget(async () =>
            {
                var file = GetFile();
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

        public override bool Equals(object obj)
        {
            if (obj is MusicFile mf)
                return this.Equals(mf);

            return false;
        }

        protected bool Equals(MusicFile other)
        {
            return FilePath == other.FilePath;
        }

        public override int GetHashCode()
        {
            return (FilePath != null ? FilePath.GetHashCode() : 0);
        }

        public static async Task<MusicFile> Create(StorageFile file)
        {
            return new MusicFile(file, await file.Properties.GetMusicPropertiesAsync());
        }
    }
}