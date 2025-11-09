using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace ShowTractor.Background
{
    public class ShowTractorBackgroundWorker
    {
        private readonly CancellationTokenSource cts = new();
        private readonly BackgroundWorkCollection backgroundWorkCollection;
        private readonly Dictionary<IBackgroundWork, DateTime> lastDoWorkTime = new();
        private Task? task;
        internal ShowTractorBackgroundWorker(BackgroundWorkCollection backgroundWorkCollection)
        {
            this.backgroundWorkCollection = backgroundWorkCollection;
            foreach (var work in backgroundWorkCollection.BackgroundWorks)
            {
                lastDoWorkTime[work] = default;
            }
        }
        public Task StopAsync()
        {
            cts.Cancel();
            return task ?? throw new InvalidOperationException($"The {nameof(ShowTractorBackgroundWorker)} has not been started.");
        }
        public void Start()
        {
            task = new Task(async () =>
                {
                while (!cts.IsCancellationRequested)
                {
                    foreach (var work in backgroundWorkCollection.BackgroundWorks)
                    {
                        cts.Token.ThrowIfCancellationRequested();
                        if (DateTime.UtcNow - lastDoWorkTime[work] > work.Interval && await work.CanDoWorkAsync())
                        {
                            lastDoWorkTime[work] = DateTime.UtcNow;
                            try
                            {
                                await work.DoWorkAsync(cts.Token);
                            }
                            catch { }
                        }
                    }
                    await Task.Delay(1000, cts.Token);
                }
            }, cts.Token);
            task.RunSynchronously();
        }
    }
}
