using System;
using System.Threading.Tasks;

namespace NetCoreAudio.Interfaces
{
    public interface IPlayer
    {
        event EventHandler PlaybackFinished;

        bool Playing { get; }
        bool Paused { get; }

        float Audio
        {

            get;
            set;
        }

        Task Play(string fileName);
        Task Pause();
        Task Resume();
        Task Stop();

    }
}
