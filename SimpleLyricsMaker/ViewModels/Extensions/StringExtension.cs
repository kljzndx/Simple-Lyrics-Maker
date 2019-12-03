using System;
using System.Linq;

namespace SimpleLyricsMaker.ViewModels.Extensions
{
    public static class StringExtension
    {
        public static string TrimExtensionName(this string input)
        {
            var strings = input.Split('.').SkipLast(1).ToArray();
            if (strings.Any())
                return String.Join('.', strings);
            return input;
        }
    }
}