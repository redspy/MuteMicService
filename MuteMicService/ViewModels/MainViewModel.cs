using MuteMicService.Controls;
using MuteMicService.Models;
using MuteMicService.Services;
using MuteMicService.Utils;
using System;
using System.Windows.Input;
using System.Windows.Threading;

namespace MuteMicService.ViewModels
{
    public class MainViewModel : ViewModelBase
    {
        private readonly MicrophoneManager _microphoneManager;
        private readonly MicMuteService _micMuteService;
        private readonly DispatcherTimer _refreshTimer;
        private string _statusText = "Initializing...";
        private string _detailText = "Starting service";
        private StatusState _statusState = StatusState.Neutral;
        private bool _serviceEnabled = false;
        private string _microphoneName = "No microphone detected";
        private bool _isMicrophoneDetected = false;
        private bool _isMicrophoneMuted = false;

        public string StatusText
        {
            get => _statusText;
            set => SetProperty(ref _statusText, value);
        }

        public string DetailText
        {
            get => _detailText;
            set => SetProperty(ref _detailText, value);
        }

        public StatusState StatusState
        {
            get => _statusState;
            set => SetProperty(ref _statusState, value);
        }

        public bool ServiceEnabled
        {
            get => _serviceEnabled;
            set
            {
                if (SetProperty(ref _serviceEnabled, value))
                {
                    if (value)
                        _micMuteService.Start();
                    else
                        _micMuteService.Stop();

                    OnPropertyChanged(nameof(ServiceButtonText));
                }
            }
        }

        public string ServiceButtonText => ServiceEnabled ? "Disable Service" : "Enable Service";

        public string MicrophoneName
        {
            get => _microphoneName;
            set => SetProperty(ref _microphoneName, value);
        }

        public bool IsMicrophoneDetected
        {
            get => _isMicrophoneDetected;
            set => SetProperty(ref _isMicrophoneDetected, value);
        }

        public bool IsMicrophoneMuted
        {
            get => _isMicrophoneMuted;
            set => SetProperty(ref _isMicrophoneMuted, value);
        }

        public ICommand ToggleServiceCommand { get; }
        public ICommand RefreshCommand { get; }

        public MainViewModel()
        {
            // Initialize managers and services
            _microphoneManager = new MicrophoneManager();
            _micMuteService = new MicMuteService(_microphoneManager);

            // Configure timer for UI refresh
            _refreshTimer = new DispatcherTimer
            {
                Interval = TimeSpan.FromSeconds(2)
            };
            _refreshTimer.Tick += RefreshTimer_Tick;

            // Configure commands
            ToggleServiceCommand = new RelayCommand(() => ServiceEnabled = !ServiceEnabled);
            RefreshCommand = new RelayCommand(RefreshMicrophoneStatus);

            // Subscribe to service events
            _micMuteService.StatusChanged += MicMuteService_StatusChanged;
            _micMuteService.MicrophoneDetectionChanged += MicMuteService_MicrophoneDetectionChanged;
            _micMuteService.MicrophoneMuteStateChanged += MicMuteService_MicrophoneMuteStateChanged;

            // Configure service parameters 
            // Check every 500ms, delay 200ms initially, retry 3 times
            _micMuteService.ConfigureSettings(500, 200, 3);

            // Initialize microphone status
            InitializeMicrophone();

            // Start UI refresh timer
            _refreshTimer.Start();
        }

        private void InitializeMicrophone()
        {
            try
            {
                bool initialized = _microphoneManager.Initialize();
                IsMicrophoneDetected = initialized;

                if (initialized)
                {
                    MicrophoneName = _microphoneManager.GetCurrentMicrophoneName();
                    IsMicrophoneMuted = _microphoneManager.IsMicrophoneMuted();
                    UpdateStatus("Microphone detected", MicrophoneName, StatusState.Success);
                }
                else
                {
                    MicrophoneName = "No microphone detected";
                    IsMicrophoneMuted = false;
                    UpdateStatus("No microphone detected", "Waiting for microphone...", StatusState.Warning);
                }
            }
            catch (Exception ex)
            {
                UpdateStatus("Error initializing", ex.Message, StatusState.Error);
            }
        }

        private void RefreshMicrophoneStatus()
        {
            try
            {
                _microphoneManager.Refresh();
                IsMicrophoneDetected = _microphoneManager.IsMicrophoneAvailable();

                if (IsMicrophoneDetected)
                {
                    MicrophoneName = _microphoneManager.GetCurrentMicrophoneName();
                    IsMicrophoneMuted = _microphoneManager.IsMicrophoneMuted();
                }
                else
                {
                    MicrophoneName = "No microphone detected";
                    IsMicrophoneMuted = false;
                }

                UpdateStatusBasedOnCurrentState();
            }
            catch (Exception ex)
            {
                UpdateStatus("Error refreshing status", ex.Message, StatusState.Error);
            }
        }

        private void UpdateStatusBasedOnCurrentState()
        {
            if (!ServiceEnabled)
            {
                UpdateStatus("Service disabled", "Toggle to enable automatic mic muting", StatusState.Inactive);
                return;
            }

            if (!IsMicrophoneDetected)
            {
                UpdateStatus("No microphone detected", "Waiting for microphone...", StatusState.Warning);
                return;
            }

            if (IsMicrophoneMuted)
            {
                UpdateStatus("Microphone muted", MicrophoneName, StatusState.Success);
            }
            else
            {
                UpdateStatus("Microphone not muted", "Attempting to mute microphone...", StatusState.Warning);
            }
        }

        private void MicMuteService_StatusChanged(object sender, string status)
        {
            // Update UI on the dispatcher thread
            App.Current.Dispatcher.Invoke(() =>
            {
                DetailText = status;
                UpdateStatusBasedOnCurrentState();
            });
        }

        private void MicMuteService_MicrophoneDetectionChanged(object sender, bool isDetected)
        {
            App.Current.Dispatcher.Invoke(() =>
            {
                IsMicrophoneDetected = isDetected;
                if (isDetected)
                {
                    MicrophoneName = _microphoneManager.GetCurrentMicrophoneName();
                }
                else
                {
                    MicrophoneName = "No microphone detected";
                    IsMicrophoneMuted = false;
                }
                UpdateStatusBasedOnCurrentState();
            });
        }

        private void MicMuteService_MicrophoneMuteStateChanged(object sender, bool isMuted)
        {
            App.Current.Dispatcher.Invoke(() =>
            {
                IsMicrophoneMuted = isMuted;
                UpdateStatusBasedOnCurrentState();
            });
        }

        private void RefreshTimer_Tick(object sender, EventArgs e)
        {
            RefreshMicrophoneStatus();
        }

        private void UpdateStatus(string status, string details, StatusState state)
        {
            StatusText = status;
            DetailText = details;
            StatusState = state;
        }

        public void Dispose()
        {
            _refreshTimer.Stop();
            _micMuteService.Stop();
            _microphoneManager.Dispose();
        }
    }
}