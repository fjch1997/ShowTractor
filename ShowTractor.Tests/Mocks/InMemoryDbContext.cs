using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using ShowTractor.Database;
using System.Data.Common;
using System.Threading.Tasks;

namespace ShowTractor.Tests.Mocks
{
    class InMemoryDbContext : ShowTractorDbContext
    {
        readonly DbConnection connection;
        private readonly bool shouldDispose;

        public InMemoryDbContext()
        {
            connection = CreateConnection();
            shouldDispose = true;
        }
        public InMemoryDbContext(DbConnection connection)
        {
            this.connection = connection;
        }
        public static DbConnection CreateConnection()
        {
            var connection = new SqliteConnection("Filename=:memory:;cache=shared;");
            connection.Open();
            return connection;
        }
        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            connection.Open();
            optionsBuilder.UseSqlite(connection);
            base.OnConfiguring(optionsBuilder);
        }
        public override void Dispose()
        {
            if (shouldDispose)
                connection.Dispose();
            base.Dispose();
        }
        public async override ValueTask DisposeAsync()
        {
            if (shouldDispose)
                connection.Dispose();
            await base.DisposeAsync();
        }
    }
}
