using System;

namespace SimpleLyricsMaker.Models
{
    public class PageModel
    {
        public PageModel(char icon, string name, Type pageType)
        {
            Icon = icon;
            Name = name;
            PageType = pageType;
        }

        public char Icon { get; }
        public string Name { get; }
        public Type PageType { get; }
    }
}