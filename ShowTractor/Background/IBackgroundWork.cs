using System;
using System.Threading;
using System.Threading.Tasks;

namespace ShowTractor.Background
{
    interface IBackgroundWork
    {
        public TimeSpan Interval { get; }
        public ValueTask<bool> CanDoWorkAsync();
        public ValueTask DoWorkAsync(CancellationToken token = default);
    }
}
