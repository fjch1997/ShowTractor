using System;

namespace ShowTractor.Interfaces
{
    interface IFactory<T>
    {
        public T Get();
    }
    public class DelegateFactory<T> : IFactory<T>
    {
        private readonly Func<T> func;
        public DelegateFactory(Func<T> func)
        {
            this.func = func;
        }
        public T Get() => func();
    }
}
