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
        private double _ringOneOpacity;

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
                    return;
                }
            }

            Task.Run(async () =>
            {
                lock (_sync)
                {
                    IsRunning = true;
                    _stopNow = false;
                    RingOneOpacity = 1.0;
                    _timeUp = false;
                    TimeLeft = TimeSpan.FromSeconds(Time);
                    _startTime = TimeSpan.FromSeconds(Time);
                    _stopwatch = Stopwatch.StartNew();
                }

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
