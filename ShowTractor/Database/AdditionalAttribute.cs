using System;
using System.ComponentModel.DataAnnotations;

namespace ShowTractor.Database
{
    class AdditionalAttribute
    {
        [Key] // See additional configuration in DbContext for composite keys.
        public Guid TvSeasonId { get; set; }
        [Key] // See additional configuration in DbContext for composite keys.
        public string AssemblyName
        {
            set => assemblyName = value;
            get => assemblyName ?? throw new InvalidOperationException("Uninitialized property: " + nameof(AssemblyName));
        }
        private string? assemblyName;
        [Key] // See additional configuration in DbContext for composite keys.
        public string Name
        {
            set => name = value;
            get => name ?? throw new InvalidOperationException("Uninitialized property: " + nameof(Name));
        }
        private string? name;
        public string Value
        {
            set => _value = value;
            get => _value ?? throw new InvalidOperationException("Uninitialized property: " + nameof(Value));
        }
        private string? _value;
        public TvSeason TvSeason
        {
            set => tvSeason = value;
            get => tvSeason ?? throw new InvalidOperationException("Uninitialized property: " + nameof(TvSeason));
        }
        private TvSeason? tvSeason;
    }
}
