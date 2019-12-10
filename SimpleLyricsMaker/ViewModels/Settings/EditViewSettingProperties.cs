using HappyStudio.UwpToolsLibrary.Auxiliarys;
using HappyStudio.UwpToolsLibrary.Auxiliarys.Attributes;

namespace SimpleLyricsMaker.ViewModels.Settings
{
    public class EditViewSettingProperties : SettingsBase
    {
        public static readonly EditViewSettingProperties Current = new EditViewSettingProperties();

        [SettingFieldByNormal(nameof(ShowAll), false)] private bool _showAll;
        [SettingFieldByNormal(nameof(LrcMadeBy), "Happy Studio")] private string _lrcMadeBy;

        private EditViewSettingProperties() : base("EditView")
        {
        }

        public bool ShowAll
        {
            get => _showAll;
            set => SetSetting(ref _showAll, value);
        }

        public string LrcMadeBy
        {
            get => _lrcMadeBy;
            set => SetSetting(ref _lrcMadeBy, value);
        }

        [SettingFieldByNormal(nameof(SplitSymbol), "$$")] private string _splitSymbol;

        public string SplitSymbol
        {
            get => _splitSymbol;
            set => SetSetting(ref _splitSymbol, value);
        }
    }
}