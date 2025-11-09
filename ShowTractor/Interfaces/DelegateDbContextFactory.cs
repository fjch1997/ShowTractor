using Microsoft.EntityFrameworkCore;
using System;

namespace ShowTractor.Interfaces
{
    internal class DelegateDbContextFactory<T> : IDbContextFactory<T> where T : DbContext
    {
        private readonly Func<T> func;
        public DelegateDbContextFactory(Func<T> func)
        {
            this.func = func;
        }
        public T CreateDbContext()
        {
            return func();
        }
    }
}
