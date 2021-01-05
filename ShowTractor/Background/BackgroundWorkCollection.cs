using System.Collections.Generic;

namespace ShowTractor.Background
{
    class BackgroundWorkCollection
    {
        public BackgroundWorkCollection(IEnumerable<IBackgroundWork> backgroundWorks)
        {
            BackgroundWorks = backgroundWorks;
        }
        public IEnumerable<IBackgroundWork> BackgroundWorks { get; set; }
    }
}
