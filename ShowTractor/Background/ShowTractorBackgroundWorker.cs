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
            task = Task.Run(async () =>
            {
                while (true)
                {
                    foreach (var work in backgroundWorkCollection.BackgroundWorks)
                    {
                        if (DateTime.UtcNow - lastDoWorkTime[work] > work.Interval && await work.CanDoWorkAsync())
                        {
                            lastDoWorkTime[work] = DateTime.UtcNow;
                            try
                            {
                                await work.DoWorkAsync();
                            }
                            catch { }
                        }
                    }
                    await Task.Delay(1000, cts.Token);
                }
            }, cts.Token);
            task.ContinueWith(t =>
            {
                if (t.IsFaulted)
                    Environment.FailFast(nameof(ShowTractorBackgroundWorker) + " failed.", t.Exception);
            });
        }
    }
}
