using Microsoft.UI.Xaml;

namespace ShowTractor.WinUI.Converters
{
    public static class MultiBooleanToVisibilityConverter
    {
        public static Visibility Convert(MultiBooleanConverterMode mode, bool value1, bool value2)
        {
            if (MultiBooleanConverter.Convert(mode, value1, value2))
                return Visibility.Visible;
            return Visibility.Collapsed;
        }
    }
}
