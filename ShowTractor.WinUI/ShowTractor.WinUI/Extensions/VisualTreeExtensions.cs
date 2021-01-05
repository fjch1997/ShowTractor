using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Media;
using System;

namespace ShowTractor.WinUI.Extensions
{
    public static class VisualTreeExtensions
    {
        public static T FindParent<T>(this DependencyObject child) where T : DependencyObject
        {
            DependencyObject parentObject = VisualTreeHelper.GetParent(child);
            if (parentObject == null) throw new InvalidOperationException("Unable to find " + nameof(T) + " in the visual tree");
            return parentObject as T ?? FindParent<T>(parentObject);
        }
    }
}
