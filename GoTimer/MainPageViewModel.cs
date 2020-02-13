using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using Plugin.SimpleAudioPlayer;
using Prism.Commands;
using Prism.Mvvm;
using Xamarin.Essentials;

namespace GoTimer
{
    class MainPageViewModel : BindableBase
    {
        private bool _stopNow = false;
        private object _sync = new object();
        private bool _timeUp = false;
        private TimeSpan _startTime = TimeSpan.Zero;
        private Stopwatch _stopwatch = null;
        private readonly ISimpleAudioPlayer _player;
        private Stream _audioStream = null;

        private int _time = 15;
        private bool _isRunning;
        private TimeSpan _timeLeft;
        private double _ringOneOpacity = 1;
        private double _ringTwoOpacity = 1;

        private int _numberOfRings = 7;
        private double _ringSevenOpacity = 1;
        private double _ringSixOpacity = 1;
        private double _ringFiveOpacity = 1;
        private double _ringFourOpacity = 1;
        private double _ringThreeOpacity = 1;

        public int Time
        {
            get => _time;
            set => SetProperty(ref _time, value);
        }

        public bool IsRunning
        {
            get => _isRunning;
            set
            {
                SetProperty(ref _isRunning, value);

                try
                {
                    MainThread.BeginInvokeOnMainThread(() =>
                    {
                        DeviceDisplay.KeepScreenOn = IsRunning;
                    });
                }
                catch (Exception ex)
                {

                }
            }
        }

        public TimeSpan TimeLeft
        {
            get => _timeLeft;
            set => SetProperty(ref _timeLeft, value);
        }

        public double RingOneOpacity
        {
            get => _ringOneOpacity;
            set => SetProperty(ref _ringOneOpacity, value);
        }

        public double RingTwoOpacity
        {
            get => _ringTwoOpacity;
            set => SetProperty(ref _ringTwoOpacity, value);
        }

        public double RingThreeOpacity
        {
            get => _ringThreeOpacity;
            set => SetProperty(ref _ringThreeOpacity, value);
        }

        public double RingFourOpacity
        {
            get => _ringFourOpacity;
            set => SetProperty(ref _ringFourOpacity, value);
        }

        public double RingFiveOpacity
        {
            get => _ringFiveOpacity;
            set => SetProperty(ref _ringFiveOpacity, value);
        }

        public double RingSixOpacity
        {
            get => _ringSixOpacity;
            set => SetProperty(ref _ringSixOpacity, value);
        }

        public double RingSevenOpacity
        {
            get => _ringSevenOpacity;
            set => SetProperty(ref _ringSevenOpacity, value);
        }

        public DelegateCommand StartCommand { get; }

        public DelegateCommand StopCommand { get; }

        public MainPageViewModel()
        {
            StartCommand = new DelegateCommand(ExecuteStartCommand);
            StopCommand = new DelegateCommand(() =>
            {
                lock(_sync)
                {
                    _stopNow = true;
                    IsRunning = false;
                    _timeUp = false;
                    StopAlarm();
                }
            });

            _player = Plugin.SimpleAudioPlayer.CrossSimpleAudioPlayer.Current;
        }

        private void ExecuteStartCommand()
        {
            lock (_sync)
            {
                if (_isRunning && !_timeUp)
                {
                    _startTime = TimeSpan.FromSeconds(Time);
                    _stopwatch = Stopwatch.StartNew();
                    RingOneOpacity = 1;
                    RingTwoOpacity = 1;
                    RingThreeOpacity = 1;
                    RingFourOpacity = 1;
                    RingFiveOpacity = 1;
                    RingSixOpacity = 1;
                    RingSevenOpacity = 1;
                    return;
                }
            }

            Task.Run(async () =>
            {
                

                lock (_sync)
                {
                    IsRunning = true;
                    _stopNow = false;
                    RingOneOpacity = 1;
                    RingTwoOpacity = 1;
                    RingThreeOpacity = 1;
                    RingFourOpacity = 1;
                    RingFiveOpacity = 1;
                    RingSixOpacity = 1;
                    RingSevenOpacity = 1;
                    _timeUp = false;
                    TimeLeft = TimeSpan.FromSeconds(Time);
                    _startTime = TimeSpan.FromSeconds(Time);
                    _stopwatch = Stopwatch.StartNew();
                }

                double segment = (_startTime.TotalMilliseconds / _numberOfRings);

                StopAlarm();

                while (!_stopNow)
                {
                    await Task.Delay(100);
                    var tempTime = _startTime - _stopwatch.Elapsed;

                    lock(_sync)
                    {
                        TimeLeft = tempTime > TimeSpan.Zero ? tempTime : TimeSpan.Zero;

                        if (TimeLeft == TimeSpan.Zero)
                        {
                            _timeUp = true;
                            SoundAlarm();
                            break;
                        }
                    }

                    int ring = (int)(_stopwatch.Elapsed.TotalMilliseconds / segment) + 1;

                    RingOneOpacity = ring == 1 ? (1 - (_stopwatch.Elapsed.TotalMilliseconds) / segment) : 0;
                    RingTwoOpacity = ring < 2 ? 1 : (ring > 2 ? 0 : (1 - (_stopwatch.Elapsed.TotalMilliseconds - segment) / segment));
                    RingThreeOpacity = ring < 3 ? 1 : (ring > 3 ? 0 : (1 - (_stopwatch.Elapsed.TotalMilliseconds - (segment * (ring - 1))) / segment));
                    RingFourOpacity = ring < 4 ? 1 : (ring > 4 ? 0 : (1 - (_stopwatch.Elapsed.TotalMilliseconds - (segment * (ring - 1))) / segment));
                    RingFiveOpacity = ring < 5 ? 1 : (ring > 5 ? 0 : (1 - (_stopwatch.Elapsed.TotalMilliseconds - (segment * (ring - 1))) / segment));
                    RingSixOpacity = ring < 6 ? 1 : (ring > 6 ? 0 : (1 - (_stopwatch.Elapsed.TotalMilliseconds - (segment * (ring - 1))) / segment));
                    RingSevenOpacity = ring < 7 ? 1 : (ring > 7 ? 0 : (1 - (_stopwatch.Elapsed.TotalMilliseconds - (segment * (ring - 1))) / segment));
                }
            });
        }

        private void SoundAlarm()
        {
            try
            {
                StopAlarm();

                _audioStream = GetStreamFromFile("GoTimer.TakeCare.mp3");

                _player.Load(_audioStream);
                _player.Loop = true;
                _player.Volume = 1;
                _player.Play();
            }
            catch (Exception ex)
            {
            }
        }

        private void StopAlarm()
        {
            if (_player.IsPlaying)
            {
                _player.Stop();
            }

            if (_audioStream != null)
            {
                _audioStream.Dispose();
                _audioStream = null;
            }
        }

        private Stream GetStreamFromFile(string filename)
        {
            var assembly = Assembly.GetExecutingAssembly();
            var stream = assembly.GetManifestResourceStream(filename);
            return stream;
        }
    }
}
