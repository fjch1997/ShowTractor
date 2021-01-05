using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Options;
using Microsoft.UI.Xaml.Data;
using Microsoft.UI.Xaml.Media.Imaging;
using ShowTractor.Pages.Details;
using System;
using System.IO;
using System.Threading.Tasks;

namespace ShowTractor.WinUI.Converters
{
    public class ArtworkToImageSourceConverter : IValueConverter, IOptions<MemoryCacheOptions>
    {
        private readonly MemoryCache cache;
        public ArtworkToImageSourceConverter()
        {
            cache = new MemoryCache(this);
        }
        public MemoryCacheOptions Value => new() { SizeLimit = 50 };
        public object? Convert(object value, Type targetType, object parameter, string language)
        {
            var artwork = (Artwork)value;
            if (artwork == null)
                return null;
            if (cache.TryGetValue(artwork.CacheKey, out var result))
                return result;
            var image = new BitmapImage();
            _ = SetSourceAsync(image, artwork);
            cache.Set(artwork.CacheKey, image, new MemoryCacheEntryOptions { Size = 1, AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(5) });
            return image;
        }

        private async Task SetSourceAsync(BitmapImage image, Artwork artwork)
        {
            using var stream = await artwork.GetStreamAsync();
            try
            {
                if (!stream.CanSeek)
                {
                    using var memoryStream = new MemoryStream();
                    await Task.Run(async () => await stream.CopyToAsync(memoryStream));
                    memoryStream.Position = 0;
                    await image.SetSourceAsync(memoryStream.AsRandomAccessStream());
                }
                else
                {
                    await image.SetSourceAsync(stream.AsRandomAccessStream());
                }
            }
            catch (OperationCanceledException)
            {
                cache.Remove(artwork.CacheKey);
            }
            catch (ObjectDisposedException)
            {
                cache.Remove(artwork.CacheKey);
            }
            catch (Exception ex)
            {
                if (artwork.CacheKey.Type == ArtworkType.Season)
                {
                    using var defaultStream = await new TvSeasonDefaultArtwork().GetStreamAsync();
                    await image.SetSourceAsync(defaultStream.AsRandomAccessStream());
                }
                else if (artwork.CacheKey.Type == ArtworkType.Episode)
                {
                    using var defaultStream = await new TvEpisodeDefaultArtwork().GetStreamAsync();
                    await image.SetSourceAsync(defaultStream.AsRandomAccessStream());
                }
                else
                {
                    throw new Exception("Failed to load default artwork.", ex);
                }
            }
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            throw new NotImplementedException();
        }
    }
}
