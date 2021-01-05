using Microsoft.UI.Xaml.Data;
using System;

namespace ShowTractor.WinUI.Converters
{
    public class ObjectToCollectionViewConverter : IValueConverter
    {
        public object? Convert(object value, Type targetType, object parameter, string language)
        {
            if (value == null)
                return null;
            var source = new CollectionViewSource
            {
                Source = value,
                IsSourceGrouped = true
            };
            return source.View;
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            throw new NotImplementedException();
        }
    }
}
