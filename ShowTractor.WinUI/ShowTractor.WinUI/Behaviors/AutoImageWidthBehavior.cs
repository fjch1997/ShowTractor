using Microsoft.UI.Xaml.Controls;
using Microsoft.Xaml.Interactivity;

namespace ShowTractor.WinUI.Behaviors
{
    public class AutoImageWidthBehavior : Behavior<Image>
    {
        protected override void OnAttached()
        {
            AssociatedObject.LayoutUpdated += AssociatedObject_LayoutUpdated;
            base.OnAttached();
        }
        protected override void OnDetaching()
        {
            AssociatedObject.LayoutUpdated -= AssociatedObject_LayoutUpdated;
            base.OnDetaching();
        }
        private void AssociatedObject_LayoutUpdated(object? sender, object e)
        {
            if (AssociatedObject.ActualHeight == 0 || AssociatedObject.ActualWidth == 0)
                return;
            AssociatedObject.Width = AssociatedObject.DesiredSize.Height * AssociatedObject.ActualWidth / AssociatedObject.ActualHeight;
        }
    }
}
