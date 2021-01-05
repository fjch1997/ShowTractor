using Microsoft.EntityFrameworkCore;
using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace ShowTractor.Database
{
    public abstract class ArtworkStream : Stream
    {
        private readonly ShowTractorDbContext context;
        private Stream? stream;
        private System.Data.Common.DbCommand? command;
        private System.Data.Common.DbDataReader? reader;

        internal ArtworkStream(ShowTractorDbContext context)
        {
            this.context = context;
        }

        internal abstract string GetCommandText();
        private async ValueTask<Stream> GetStreamAsync()
        {
            if (stream != null)
                return stream;

            var connection = context.Database.GetDbConnection();
            await connection.OpenAsync();
            command = connection.CreateCommand();
            var reader = await command.ExecuteReaderAsync();
            if (!await reader.ReadAsync())
                throw new ArtworkNotFoundException();
            stream = reader.GetStream(0);
            return stream;
        }
        private Stream GetStream()
        {
            if (stream != null)
                return stream;

            var connection = context.Database.GetDbConnection();
            connection.Open();
            command = connection.CreateCommand();
            command.CommandText = GetCommandText();
            reader = command.ExecuteReader();
            if (!reader.Read() || reader.IsDBNull(0))
                throw new ArtworkNotFoundException();
            stream = reader.GetStream(0);
            return stream;
        }
        public override bool CanRead => GetStream().CanRead;
        public override bool CanSeek => GetStream().CanSeek;
        public override bool CanWrite => false;
        public override long Length => GetStream().Length;
        public override long Position { get => GetStream().Position; set => GetStream().Position = value; }
        public override void Flush() => GetStream().Flush();
        public async override Task FlushAsync(CancellationToken cancellationToken) => await (await GetStreamAsync()).FlushAsync(cancellationToken);
        public override int Read(byte[] buffer, int offset, int count)
        {
            return GetStream().Read(buffer, offset, count);
        }
        public async override Task<int> ReadAsync(byte[] buffer, int offset, int count, CancellationToken cancellationToken)
        {
            return await (await GetStreamAsync()).ReadAsync(buffer.AsMemory(offset, count), cancellationToken);
        }
        public async override ValueTask<int> ReadAsync(Memory<byte> buffer, CancellationToken cancellationToken = default)
        {
            return await (await GetStreamAsync()).ReadAsync(buffer, cancellationToken);
        }
        public override long Seek(long offset, SeekOrigin origin)
        {
            return GetStream().Seek(offset, origin);
        }
        public override void SetLength(long value)
        {
            GetStream().SetLength(value);
        }
        public override void Write(byte[] buffer, int offset, int count)
        {
            throw new NotSupportedException();
        }
        protected override void Dispose(bool disposing)
        {
            context.Dispose();
            command?.Dispose();
            stream?.Dispose();
            reader?.Dispose();
            base.Dispose(disposing);
        }
        public async override ValueTask DisposeAsync()
        {
            await context.DisposeAsync();
            if (command != null)
                await command.DisposeAsync();
            if (stream != null)
                await stream.DisposeAsync();
            if (reader != null)
                await reader.DisposeAsync();
            await base.DisposeAsync();
        }
    }
    public class TvSeasonArtworkStream : ArtworkStream
    {
        private readonly ShowTractorDbContext context;
        private readonly Guid tvSeasonId;
        internal TvSeasonArtworkStream(ShowTractorDbContext context, Guid tvSeasonId) : base(context)
        {
            this.context = context;
            this.tvSeasonId = tvSeasonId;
        }
        internal override string GetCommandText()
        {
            string tableName = context.Model.FindEntityType(typeof(TvSeason)).GetTableName();
            return $"SELECT {nameof(TvSeason.Artwork)} FROM {tableName} WHERE {nameof(TvSeason.Id)} = '{tvSeasonId.ToString().ToUpperInvariant()}'";
        }
    }
    public class TvEpisodeArtworkStream : ArtworkStream
    {
        private readonly Guid tvSeasonId;
        private readonly int episodeNumber;
        private readonly ShowTractorDbContext context;
        internal TvEpisodeArtworkStream(ShowTractorDbContext context, Guid tvSeasonId, int episodeNumber) : base(context)
        {
            this.context = context;
            this.tvSeasonId = tvSeasonId;
            this.episodeNumber = episodeNumber;
        }
        internal override string GetCommandText()
        {
            string tableName = context.Model.FindEntityType(typeof(TvEpisode)).GetTableName();
            return $"SELECT {nameof(TvEpisode.Artwork)} FROM {tableName} WHERE {nameof(TvEpisode.TvSeasonId)} = '{tvSeasonId.ToString().ToUpperInvariant()}' AND {nameof(TvEpisode.EpisodeNumber)} = {episodeNumber}";
        }
    }
    public class ArtworkNotFoundException : Exception { }
}
