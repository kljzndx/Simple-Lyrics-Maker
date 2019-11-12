using System;
using System.Threading.Tasks;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using SimpleLyricsMaker.Logs;
using SimpleLyricsMaker.ViewModels;

// https://go.microsoft.com/fwlink/?LinkId=402352&clcid=0x804 上介绍了“空白页”项模板

namespace SimpleLyricsMaker.Views
{
    /// <summary>
    /// 可用于自身或导航至 Frame 内部的空白页。
    /// </summary>
    public sealed partial class UIFramework : Page
    {
        private MainViewModel _vm;

        public UIFramework()
        {
            this.InitializeComponent();
            _vm = (MainViewModel) this.DataContext;
        }
    }
}
