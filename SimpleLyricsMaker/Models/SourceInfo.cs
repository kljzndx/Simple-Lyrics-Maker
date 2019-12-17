using Windows.Storage;
using HappyStudio.Parsing.Subtitle.LRC;

namespace SimpleLyricsMaker.Models
{
    public class SourceInfo
    {
        public SourceInfo(StorageFolder folder, MusicFile musicFile, LrcBlock lrcBlock)
        {
            Folder = folder;
            MusicFile = musicFile;
            LrcBlock = lrcBlock;
        }

        public StorageFolder Folder { get; }
        public MusicFile MusicFile { get; }
        public LrcBlock LrcBlock { get; }
    }
}