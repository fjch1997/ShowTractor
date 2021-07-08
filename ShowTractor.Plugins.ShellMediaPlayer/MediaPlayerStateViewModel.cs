using ShowTractor.Plugins.Interfaces;
using System;
using System.Diagnostics;

namespace ShowTractor.Plugins.ShellMediaPlayer
{
    public class ShellMediaPlayerStateViewModel : MediaPlayerStateViewModel
    {
        public ShellMediaPlayerStateViewModel(Process process)
        {
            if (process == null) /// <see cref="Process.Start(ProcessStartInfo)"/> may return null in some circumstances.
                State = MediaPlayerState.Closed;
            else if (process.HasExited)
                State = MediaPlayerState.Closed;
            else
                process.Exited += Process_Exited;
        }

        private void Process_Exited(object sender, EventArgs e)
        {
            State = MediaPlayerState.Closed;
        }
    }
}
