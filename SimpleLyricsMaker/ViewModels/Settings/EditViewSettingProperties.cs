using HappyStudio.UwpToolsLibrary.Auxiliarys;
using HappyStudio.UwpToolsLibrary.Auxiliarys.Attributes;

namespace SimpleLyricsMaker.ViewModels.Settings
{
    public class EditViewSettingProperties : SettingsBase
    {
        public static readonly EditViewSettingProperties Current = new EditViewSettingProperties();

        [SettingFieldByNormal(nameof(ShowAll), false)] private bool _showAll;

        private EditViewSettingProperties() : base("EditView")
        {
        }

        public bool ShowAll
        {
            get => _showAll;
            set => SetSetting(ref _showAll, value);
        }

        [SettingFieldByNormal(nameof(LrcMadeBy), "Happy Studio")] private string _lrcMadeBy;

        public string LrcMadeBy
        {
            get => _lrcMadeBy;
            set => SetSetting(ref _lrcMadeBy, value);
        }
    }
}