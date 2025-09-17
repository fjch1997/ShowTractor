using Microsoft.UI.Xaml.Data;
using Microsoft.UI.Xaml.Media.Imaging;
using ShowTractor.Interfaces;
using System;
using System.IO;

namespace ShowTractor.WinUI.Converters
{
    public class StreamToImageSourceConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            var stream = (IFactory<Stream>)value;
            if (stream != null)
            {
                var image = new BitmapImage();
                image.SetSource(stream.Get().AsRandomAccessStream());
                return image;
            };

            return null;
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            throw new NotImplementedException();
        }
    }
}
