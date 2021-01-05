using ShowTractor.Plugins.Tmdb.Properties;
using System;

namespace ShowTractor.Plugins.Tmdb
{
    public abstract class TmdbException : Exception
    {
        protected TmdbException(string message) : base(message) { }
    }
    public class ApiKeyNotSetException : TmdbException
    {
        public ApiKeyNotSetException() : base(Resources.ApiKeyNotSetErrorMessage) { }
    }
    public class ShowNotFoundException : TmdbException
    {
        public ShowNotFoundException(string showName) : base(string.Format(Resources.ShowNotFoundErrorMessage, showName)) { }
    }
}
