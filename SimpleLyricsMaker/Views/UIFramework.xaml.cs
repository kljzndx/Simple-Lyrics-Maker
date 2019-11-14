using System;
using System.Linq;
using System.Threading.Tasks;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using SimpleLyricsMaker.Logs;
using SimpleLyricsMaker.Models;
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

        private void OpenPane_Button_OnClick(object sender, RoutedEventArgs e)
        {
            Root_SplitView.IsPaneOpen = !Root_SplitView.IsPaneOpen;
        }

        private void HamburgMenu_ListBox_OnSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            PageModel pm = e.AddedItems.FirstOrDefault() as PageModel;
            if (pm?.PageType != null)
                Main_Frame.Navigate(pm.PageType);
        }
    }
}
