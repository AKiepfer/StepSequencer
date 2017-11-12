using System;
using System.Diagnostics;
using System.Runtime;
using System.Threading;
using Windows.System.Threading;

namespace ManagedAudioEngineUniversal.Model
{
    public class Transporter
    {
        private readonly int _steps;
        private readonly Action<int> _playAction;
        private readonly ManualResetEvent _pauseEvent;
        private readonly ManualResetEvent _stopEvent;

        private double _milliseconds;

        public Transporter(int steps, Action<int> playAction)
        {
            _steps = steps;
            _playAction = playAction;

            _pauseEvent = new ManualResetEvent(false);
            _stopEvent = new ManualResetEvent(false);

            Activate();
        }
        
        public double Bpm
        {
            set
            {
                double currentValue = _milliseconds;
                double computed = ((60000.0/value)/8);//* 1.5;

                Interlocked.CompareExchange(ref _milliseconds, computed, currentValue);
            }
        }

        public void Start()
        {
            _pauseEvent.Set();
            _stopEvent.Set();
        }

        public void Stop()
        {
            _stopEvent.Reset();

            GC.Collect();
            GC.WaitForPendingFinalizers();
        }

        public void Pause()
        {
            _pauseEvent.Reset();

            GC.Collect();
            GC.WaitForPendingFinalizers();
        }

        private void Activate()
        {
            GCSettings.LatencyMode = GCLatencyMode.SustainedLowLatency;

            const double timerResolution = 20;

            ThreadPool.RunAsync(operation =>
            {
                var delayEvent = new ManualResetEvent(false);
                var stopWatch = new Stopwatch();
                stopWatch.Start();

                double currentTimerResolution = timerResolution;

                int nextValue = 0;
                while (true)
                {
                    _pauseEvent.WaitOne(Timeout.InfiniteTimeSpan);

                    if (!_stopEvent.WaitOne(0))
                    {
                        nextValue = 0;
                    }

                    _stopEvent.WaitOne(Timeout.InfiniteTimeSpan);

                    //time.delay((int)(3600 / MasterBpm) * 2);

                    //milliseconds we have to wait
                    double millis = _milliseconds;

                    //high resolution ticks we want to have
                    double wantedTicks = millis * TimeSpan.TicksPerMillisecond;//(Stopwatch.Frequency / 1000.0);

                    //delay we can wait with a simple sleep (windows timer resolution 16ms)
                    var delay = millis - currentTimerResolution;

                    //wait 
                    delayEvent.WaitOne(TimeSpan.FromMilliseconds(delay));

                    //spin the rest
                    long elapsed;
                    do
                    {
                        elapsed = stopWatch.Elapsed.Ticks;
                    } while (elapsed < wantedTicks);

                    var value = nextValue;

                    _playAction(value);

                    stopWatch.Restart();

                    nextValue++;

                    if (nextValue == _steps)
                        nextValue = 0;
                }
            },
                WorkItemPriority.High,
                WorkItemOptions.TimeSliced);

            // _timerThread = ThreadPool.RunAsync(operation =>
            // {
            //     var delayEvent = new ManualResetEvent(false);
            //     var stopWatch = new Stopwatch();
            //     stopWatch.Start();

            //     int nextValue = 0;
            //     while (true)
            //     {
            //         _pauseEvent.WaitOne(Timeout.InfiniteTimeSpan);

            //         if (!_stopEvent.WaitOne(0))
            //         {
            //             nextValue = 0;
            //         }

            //         _stopEvent.WaitOne(Timeout.InfiniteTimeSpan);

            //         double wantedTicks = (((3600 / MasterBpm) * 2) / 1000) *
            //                              Stopwatch.Frequency;

            //         SpinWait.SpinUntil(() => stopWatch.ElapsedTicks >= wantedTicks,
            //              TimeSpan.FromMilliseconds(((3600 / MasterBpm)) * 2));


            //         stopWatch.Restart();

            //         var value = nextValue;

            //         PlayTracks(value);

            //         nextValue++;

            //         if (nextValue == 32)
            //             nextValue = 0;
            //     }
            // },
            //WorkItemPriority.High,
            //WorkItemOptions.TimeSliced);
        }
    }
}
