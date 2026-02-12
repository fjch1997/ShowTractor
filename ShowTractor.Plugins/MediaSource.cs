using System;

namespace ShowTractor.Plugins.Interfaces
{
    public class MediaSource
    {
        public string DisplayName { get; set; } = string.Empty;
        public string ShowName { get; set; } = string.Empty;
        public int Season { get; set; }
        public int EpisodeNumber { get; set; }
        /// <summary>
        /// Whether the media source is immediately playable. If it is, a play button should be directly shown in the UI.
        /// </summary>
        public bool Playable { get; set; }
        /// <summary>
        /// The remote URI of the media source. The protocol will be used to determine which Download Manager or Media Player to use.
        /// </summary>
        public Uri? Uri { get; set; }
        /// <summary>
        /// The size of the media in bytes. If zero, size will be hidden in the UI.
        /// </summary>
        public long Bytes { get; set; }
    }
}
