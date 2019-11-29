using System;
using System.Threading.Tasks;
using Windows.Storage;
using Windows.Storage.FileProperties;

namespace SimpleLyricsMaker.Models
{
    public class MusicFile
    {
        private MusicFile(StorageFile file, StorageItemThumbnail albumCover, MusicProperties musicProperties)
        {
            File = file;
            AlbumCover = albumCover;

            Title = musicProperties.Title;
            Artist = musicProperties.Artist;
            Album = musicProperties.Album;
        }

        public string Title { get; }
        public string Artist { get; }
        public string Album { get; }
        public StorageItemThumbnail AlbumCover { get; }
        public StorageFile File { get; }

        public static async Task<MusicFile> Create(StorageFile file)
        {
            return new MusicFile(file, await file.GetThumbnailAsync(ThumbnailMode.MusicView), await file.Properties.GetMusicPropertiesAsync());
        }
    }
}