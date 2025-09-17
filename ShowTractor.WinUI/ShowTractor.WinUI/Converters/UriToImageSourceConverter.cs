using Microsoft.UI.Xaml.Data;
using Microsoft.UI.Xaml.Media.Imaging;
using ShowTractor.Pages.Details;
using System;
using System.IO;
using System.Reflection;

namespace ShowTractor.WinUI.Converters
{
    public enum ArtworkType
    {
        Season, Episode
    }

    public class UriToImageSourceConverter : IValueConverter
    {
        public ArtworkType ArtworkType { get; set; }
        private Stream GetStreamByType()
        {
            var assembly = Assembly.GetAssembly(typeof(IArtworkService));
            if (assembly == null)
                throw new InvalidOperationException("Could not load ShowTractor assembly.");
            switch (ArtworkType)
            {
                case ArtworkType.Season:
                    return assembly.GetManifestResourceStream("ShowTractor.Assets.poster-placeholder.jpg")
                            ?? throw new InvalidOperationException("Failed to load image from manifest resources.");
                case ArtworkType.Episode:
                    return assembly.GetManifestResourceStream("ShowTractor.Assets.episode-placeholder.jpg")
                            ?? throw new InvalidOperationException("Failed to load image from manifest resources.");
                default:
                    throw new InvalidOperationException();
            }
        }
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            var uri = (Uri)value;
            if (uri == null)
            {
                var image = new BitmapImage();
                image.SetSource(GetStreamByType().AsRandomAccessStream());
                return image;
            };

            return new BitmapImage(uri);
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            throw new NotImplementedException();
        }
    }
}
