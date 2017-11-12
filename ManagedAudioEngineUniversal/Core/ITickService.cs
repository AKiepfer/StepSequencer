using System;
using System.Reactive;

namespace ManagedAudioEngineUniversal.Core
{
    public interface ITickService
    {
        IObservable<Timestamped<int>> Ticks { get; }

        void Start();

        void Stop();
    }
}