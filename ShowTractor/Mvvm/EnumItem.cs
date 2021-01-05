using System;
using System.ComponentModel.DataAnnotations;

namespace ShowTractor.Mvvm
{
    public class EnumItem
    {
        public EnumItem(Enum value)
        {
            Value = value;
            if (value == null)
            {
                DisplayText = string.Empty;
                return;
            }
            var enumType = value.GetType();
            if (!enumType.IsEnum)
                throw new InvalidOperationException(enumType.FullName + " is not an Enum.");
            var member = enumType.GetMember(value.ToString() ?? throw new Exception());
            var display = (DisplayAttribute)member[0].GetCustomAttributes(typeof(DisplayAttribute), false)[0];
            if (display.Name == null)
            {
                DisplayText = string.Empty;
            }
            else if (display.ResourceType != null)
            {
                var propertyInfo = display.ResourceType.GetProperty(display.Name, System.Reflection.BindingFlags.Static | System.Reflection.BindingFlags.Public);
                if (propertyInfo == null)
                    throw new InvalidOperationException($"Property {display.Name} does not exist in {display.ResourceType.FullName}.");
                var getMethod = propertyInfo.GetGetMethod() ?? throw new InvalidOperationException($"Property {display.Name} does not have a public get method in {display.ResourceType.FullName}.");
                DisplayText = (string?)getMethod.Invoke(null, null) ?? string.Empty;
            }
            else
            {
                DisplayText = display.Name;
            }
        }

        public Enum Value { get; }
        public string DisplayText { get; }
    }
}
