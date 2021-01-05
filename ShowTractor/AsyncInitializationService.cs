using Microsoft.EntityFrameworkCore;
using ShowTractor.Database;
using System.Threading.Tasks;

namespace ShowTractor
{
    public interface IAsyncInitializationService
    {
        Task Task { get; }
    }

    public class AsyncInitializationService : IAsyncInitializationService
    {
        internal AsyncInitializationService(ShowTractorDbContext context)
        {
            Task = Task.Run(async () =>
            {
                using (context)
                {
                    await context.Database.MigrateAsync();
                }
            });
        }
        public Task Task { get; }
    }
}
