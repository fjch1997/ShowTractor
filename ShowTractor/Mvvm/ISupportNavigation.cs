namespace ShowTractor.Mvvm
{
    public interface ISupportNavigation : ISupportNavigationParameter
    {
        /// <summary>
        /// Invoked immediately before the Page is unloaded and is no longer the current source of a parent Frame.
        /// </summary>
        /// <returns>True, if cancellation is requested.</returns>
        public bool OnNavigatingFrom() => false;
        /// <summary>
        /// Invoked immediately after the Page is unloaded and is no longer the current source of a parent Frame.
        /// </summary>
        public void OnNavigatedFrom() { }
        /// <summary>
        /// Invoked when the Page is loaded and becomes the current source of a parent Frame.
        /// </summary>
        public void OnNavigatedTo(object parameter) { Parameter = parameter; }
    }

    public interface ISupportNavigationParameter
    {
        object? Parameter { get; set; }
    }
}