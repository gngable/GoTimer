using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Drawing;
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
        private Color _backgroundColor = Color.LightSkyBlue;
        private Color _textColor = Color.Black;
        private Color _ringOneColor = Color.DeepPink;
        private Color _ringTwoColor = Color.Pink;
        private Color _ringThreeColor = Color.Purple;
        private Color _ringFourColor = Color.MediumPurple;
        private Color _ringFiveColor = Color.DarkTurquoise;
        private Color _ringSixColor = Color.MediumTurquoise;
        private Color _ringSevenColor = Color.PaleTurquoise;
        private string _selectedTheme = "Colorful";
        private string _selectedSound = "Cardinal";

        public Color BackgroundColor
        {
            get => _backgroundColor;
            set => SetProperty(ref _backgroundColor, value);
        }

        public Color TextColor
        {
            get => _textColor;
            set => SetProperty(ref _textColor, value);
        }

        public Color RingOneColor
        {
            get => _ringOneColor;
            set => SetProperty(ref _ringOneColor, value);
        }

        public Color RingTwoColor
        {
            get => _ringTwoColor;
            set => SetProperty(ref _ringTwoColor, value);
        }

        public Color RingThreeColor
        {
            get => _ringThreeColor;
            set => SetProperty(ref _ringThreeColor, value);
        }

        public Color RingFourColor
        {
            get => _ringFourColor;
            set => SetProperty(ref _ringFourColor, value);
        }

        public Color RingFiveColor
        {
            get => _ringFiveColor;
            set => SetProperty(ref _ringFiveColor, value);
        }

        public Color RingSixColor
        {
            get => _ringSixColor;
            set => SetProperty(ref _ringSixColor, value);
        }

        public Color RingSevenColor
        {
            get => _ringSevenColor;
            set => SetProperty(ref _ringSevenColor, value);
        }

        public int Time
        {
            get => _time;
            set
            {
                SetProperty(ref _time, value);

                try
                {
                    GoTimerStatic.SaveProperty(nameof(Time), Time);
                }
                catch (Exception e)
                {
                    
                }
            }
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

        public List<string> Themes => new List<string>(new []{"Colorful", "Rainbow", "Smoke", "Dark", "Evie's"});

        public List<string> Sounds => new List<string>(new[] { "Beep Beep", "Evacuate", "Air Horn", "Cardinal", "Hawk", "School Bell", "Tolling Bell", "Train Horn"});


        public string SelectedTheme
        {
            get => _selectedTheme;
            set
            {
                SetProperty(ref _selectedTheme, value);
                SetTheme();

                try
                {
                    GoTimerStatic.SaveProperty(nameof(SelectedTheme), SelectedTheme);
                }
                catch (Exception e)
                {

                }
            }
        }

        public string SelectedSound
        {
            get => _selectedSound;
            set
            {
                SetProperty(ref _selectedSound, value);

                try
                {
                    GoTimerStatic.SaveProperty(nameof(SelectedSound), SelectedSound);
                }
                catch (Exception e)
                {

                }
            }
        }

        public TimeSpan TimeLeft
        {
            get => _timeLeft;
            set
            {
                SetProperty(ref _timeLeft, value);
                RaisePropertyChanged(nameof(TimeText));
            }
        }

        public string TimeText => $"{TimeLeft.Minutes % 100 :D2}:{TimeLeft.Seconds % 100:D2}:{TimeLeft.Milliseconds % 100:D2}";

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
            StopCommand = new DelegateCommand(ExecuteStopCommand);

            _player = Plugin.SimpleAudioPlayer.CrossSimpleAudioPlayer.Current;

            try
            {
                if (GoTimerStatic.HasProperty(nameof(Time)))
                {
                    Time = (int)GoTimerStatic.GetProperty(nameof(Time));
                }

                if (GoTimerStatic.HasProperty(nameof(SelectedTheme)))
                {
                    SelectedTheme = (string)GoTimerStatic.GetProperty(nameof(SelectedTheme));
                }

                if (GoTimerStatic.HasProperty(nameof(SelectedSound)))
                {
                    SelectedSound = (string)GoTimerStatic.GetProperty(nameof(SelectedSound));
                }
            }
            catch (Exception ex)
            {

            }
        }

        private void ExecuteStopCommand()
        {
            if (_timeUp)
            {
                RingOneOpacity = 1;
                RingTwoOpacity = 1;
                RingThreeOpacity = 1;
                RingFourOpacity = 1;
                RingFiveOpacity = 1;
                RingSixOpacity = 1;
                RingSevenOpacity = 1;
            }

            lock (_sync)
            {
                _stopNow = true;
                IsRunning = false;
                _timeUp = false;
                StopAlarm();
            }
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

                if (_stopNow)
                {
                    RingOneOpacity = 1;
                    RingTwoOpacity = 1;
                    RingThreeOpacity = 1;
                    RingFourOpacity = 1;
                    RingFiveOpacity = 1;
                    RingSixOpacity = 1;
                    RingSevenOpacity = 1;
                }
            });
        }

        private void SoundAlarm()
        {
            try
            {
                StopAlarm();

                _audioStream = GetStreamFromFile($"GoTimer.Sounds.{SelectedSound.Replace(" ", "")}.mp3");

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

        private void SetTheme()
        {
            if (SelectedTheme == "Colorful")
            {
                BackgroundColor = Color.LightSkyBlue;
                TextColor = Color.Black;
                RingOneColor = Color.DeepPink;
                RingTwoColor = Color.Pink;
                RingThreeColor = Color.Purple;
                RingFourColor = Color.MediumPurple;
                RingFiveColor = Color.DarkTurquoise;
                RingSixColor = Color.MediumTurquoise;
                RingSevenColor = Color.PaleTurquoise;
            }
            else if (SelectedTheme == "Rainbow")
            {
                BackgroundColor = Color.SkyBlue;
                TextColor = Color.Black;
                RingOneColor = Color.Red;
                RingTwoColor = Color.FromArgb(255, 127, 0);
                RingThreeColor = Color.Yellow;
                RingFourColor = Color.FromArgb(0, 255, 0);
                RingFiveColor = Color.Blue;
                RingSixColor = Color.FromArgb(46, 43, 95);
                RingSevenColor = Color.FromArgb(139, 0, 255);
            }
            else if (SelectedTheme == "Smoke")
            {
                BackgroundColor = Color.GhostWhite;
                TextColor = Color.Black;
                RingOneColor = Color.Gainsboro;
                RingTwoColor = Color.DarkGray;
                RingThreeColor = Color.Gray;
                RingFourColor = Color.DimGray;
                RingFiveColor = Color.Gray;
                RingSixColor = Color.DarkGray;
                RingSevenColor = Color.Gray;
            }
            else if (SelectedTheme == "Dark")
            {
                BackgroundColor = Color.Black;
                TextColor = Color.White;
                RingOneColor = Color.DimGray;
                RingTwoColor = Color.DimGray;
                RingThreeColor = Color.DimGray;
                RingFourColor = Color.DimGray;
                RingFiveColor = Color.DimGray;
                RingSixColor = Color.DimGray;
                RingSevenColor = Color.DimGray;
            }
            else if (SelectedTheme == "Evie's")
            {
                BackgroundColor = Color.DarkBlue;
                TextColor = Color.GreenYellow;
                RingOneColor = Color.LightGreen;
                RingTwoColor = Color.PaleTurquoise;
                RingThreeColor = Color.Turquoise;
                RingFourColor = Color.Turquoise;
                RingFiveColor = Color.DarkTurquoise;
                RingSixColor = Color.DarkTurquoise;
                RingSevenColor = Color.DodgerBlue;
            }
        }
    }
}
