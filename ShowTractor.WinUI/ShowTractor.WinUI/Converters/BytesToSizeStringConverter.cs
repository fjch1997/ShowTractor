using Microsoft.UI.Xaml.Data;
using System;
using System.Runtime.InteropServices;
using System.Text;

namespace ShowTractor.WinUI.Converters
{
    public class BytesToSizeStringConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            var sb = new StringBuilder(32);
            StrFormatByteSizeW((long)value, sb, sb.Capacity);
            return sb.ToString();
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            throw new NotImplementedException();
        }

        [DllImport("shlwapi.dll", CharSet = CharSet.Unicode)]
        private static extern long StrFormatByteSizeW(long qdw, [MarshalAs(UnmanagedType.LPWStr)] StringBuilder pszBuf, int cchBuf);
    }
}
