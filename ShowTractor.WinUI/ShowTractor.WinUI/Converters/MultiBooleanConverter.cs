using System;

namespace ShowTractor.WinUI.Converters
{
    public enum MultiBooleanConverterMode
    {
        And, Or
    }
    public static class MultiBooleanConverter
    {
        public static bool Convert(MultiBooleanConverterMode mode, bool value1, bool value2)
        {
            return mode switch
            {
                MultiBooleanConverterMode.And => value1 && value2,
                MultiBooleanConverterMode.Or => value1 || value2,
                _ => throw new InvalidOperationException(),
            };
        }
    }
}
