using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media;
using Microsoft.Xaml.Interactivity;
using System;

namespace ShowTractor.WinUI.Behaviors
{
    public sealed class NavigateToPageAction : DependencyObject, IAction
    {
        public Type TargetPageType
        {
            get { return (Type)GetValue(TargetPageTypeProperty); }
            set { SetValue(TargetPageTypeProperty, value); }
        }
        public static readonly DependencyProperty TargetPageTypeProperty =
            DependencyProperty.Register("TargetPageType", typeof(Type), typeof(NavigateToPageAction), new PropertyMetadata(null));

        public object Parameter
        {
            get { return (object)GetValue(ParameterProperty); }
            set { SetValue(ParameterProperty, value); }
        }
        public static readonly DependencyProperty ParameterProperty =
            DependencyProperty.Register("Parameter", typeof(object), typeof(NavigateToPageAction), new PropertyMetadata(null));

        public Frame Frame
        {
            get { return (Frame)GetValue(FrameProperty); }
            set { SetValue(FrameProperty, value); }
        }
        public static readonly DependencyProperty FrameProperty =
            DependencyProperty.Register("Frame", typeof(Frame), typeof(NavigateToPageAction), new PropertyMetadata(null));

        /// <summary>
        /// Executes the action.
        /// </summary>
        /// <param name="sender">The <see cref="System.Object"/> that is passed to the action by the behavior. Generally this is <seealso cref="Microsoft.Xaml.Interactivity.IBehavior.AssociatedObject"/> or a target object.</param>
        /// <param name="parameter">The value of this parameter is determined by the caller.</param>
        /// <returns>True if the navigation to the specified page is successful; else false.</returns>
        public object Execute(object sender, object parameter)
        {
            if (TargetPageType  == null)
            {
                return false;
            }
            
            if (Frame != null)
            {
                return Frame.Navigate(TargetPageType, this.Parameter ?? parameter);
            }

            INavigate? navigateElement = Window.Current?.Content as INavigate;
            DependencyObject? senderObject = sender as DependencyObject;

            // If the sender wasn't an INavigate, then keep looking up the tree from the
            // root we were given for another INavigate.
            while (senderObject != null && navigateElement == null)
            {
                navigateElement = senderObject as INavigate;
                if (navigateElement == null)
                {
                    senderObject = VisualTreeHelper.GetParent(senderObject);
                }
            }

            if (navigateElement == null)
            {
                return false;
            }

            Frame? frame = navigateElement as Frame;

            if (frame != null)
            {
                return frame.Navigate(TargetPageType, this.Parameter ?? parameter);
            }
            else
            {
                return navigateElement.Navigate(TargetPageType);
            }
        }
    }
}
