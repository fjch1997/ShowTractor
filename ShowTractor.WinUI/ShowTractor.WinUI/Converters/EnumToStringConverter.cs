using Microsoft.UI.Xaml.Data;
using System;
using System.ComponentModel.DataAnnotations;

namespace ShowTractor.WinUI.Converters
{
    public class EnumToStringConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            if (value == null)
                return string.Empty;
            var enumType = value.GetType();
            if (!enumType.IsEnum)
                throw new InvalidOperationException(enumType.FullName + " is not an Enum.");
            var member = enumType.GetMember(value.ToString() ?? throw new Exception());
            var display = (DisplayAttribute)member[0].GetCustomAttributes(typeof(DisplayAttribute), false)[0];
            if (display.Name == null)
            {
                return string.Empty;
            }
            else if (display.ResourceType != null)
            {
                var propertyInfo = display.ResourceType.GetProperty(display.Name, System.Reflection.BindingFlags.Static | System.Reflection.BindingFlags.Public);
                if (propertyInfo == null)
                    throw new InvalidOperationException($"Property {display.Name} does not exist in {display.ResourceType.FullName}.");
                var getMethod = propertyInfo.GetGetMethod() ?? throw new InvalidOperationException($"Property {display.Name} does not have a public get method in {display.ResourceType.FullName}.");
                return getMethod.Invoke(null, null) ?? string.Empty;
            }
            else
            {
                return display.Name;
            }
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            throw new NotImplementedException();
        }
    }
}
