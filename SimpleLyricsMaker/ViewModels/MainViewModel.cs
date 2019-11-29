using System.Collections.ObjectModel;
using GalaSoft.MvvmLight;
using SimpleLyricsMaker.Models;
using SimpleLyricsMaker.Views;

namespace SimpleLyricsMaker.ViewModels
{
    public class MainViewModel : ViewModelBase
    {
        public MainViewModel()
        {
            PagesList = new ObservableCollection<PageModel>
            {
                new PageModel('\uE142', "信息编辑", typeof(EditPage)),
                new PageModel('\uE15E', "字幕打点", null),
                new PageModel('\uE052', "完成预览", null),
            };
        }

        public ObservableCollection<PageModel> PagesList { get; }
    }
}