using MuteMicService.Models;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace MuteMicService.Services
{
    public class MicMuteService
    {
        private readonly MicrophoneManager _microphoneManager;
        private CancellationTokenSource _cancellationTokenSource;
        private Task _monitoringTask;
        private readonly object _lock = new object();
        private bool _isRunning = false;
        private int _checkIntervalMs = 1000; // Check every second by default
        private int _initialDelayMs = 500;   // Initial delay before checking
        private int _retryAttempts = 3;      // Number of retry attempts

        public event EventHandler<string> StatusChanged;
        public event EventHandler<bool> MicrophoneDetectionChanged;
        public event EventHandler<bool> MicrophoneMuteStateChanged;

        public bool IsRunning
        {
            get { return _isRunning; }
            private set
            {
                if (_isRunning != value)
                {
                    _isRunning = value;
                    OnStatusChanged($"Service is {(value ? "running" : "stopped")}");
                }
            }
        }

        public MicMuteService(MicrophoneManager microphoneManager)
        {
            _microphoneManager = microphoneManager ?? throw new ArgumentNullException(nameof(microphoneManager));
        }

        public void Start()
        {
            lock (_lock)
            {
                if (IsRunning)
                    return;

                _cancellationTokenSource = new CancellationTokenSource();
                _monitoringTask = Task.Run(() => MonitorMicrophoneAsync(_cancellationTokenSource.Token));
                IsRunning = true;
            }
        }

        public void Stop()
        {
            lock (_lock)
            {
                if (!IsRunning)
                    return;

                _cancellationTokenSource?.Cancel();
                _monitoringTask?.Wait();
                IsRunning = false;
            }
        }

        private async Task MonitorMicrophoneAsync(CancellationToken cancellationToken)
        {
            bool wasMicAvailable = false;

            while (!cancellationToken.IsCancellationRequested)
            {
                try
                {
                    // Sleep before checking to allow for initialization
                    await Task.Delay(_initialDelayMs, cancellationToken);

                    // Check if microphone is available
                    bool isMicAvailable = _microphoneManager.Initialize();

                    // If mic state changed, notify
                    if (wasMicAvailable != isMicAvailable)
                    {
                        wasMicAvailable = isMicAvailable;
                        OnMicrophoneDetectionChanged(isMicAvailable);

                        if (isMicAvailable)
                            OnStatusChanged($"Microphone detected: {_microphoneManager.GetCurrentMicrophoneName()}");
                        else
                            OnStatusChanged("No microphone detected");
                    }

                    // If mic is available, ensure it's muted
                    if (isMicAvailable)
                    {
                        bool wasMuted = _microphoneManager.IsMicrophoneMuted();
                        bool muteSuccess = false;

                        // Try to mute multiple times in case of failure
                        for (int attempt = 0; attempt < _retryAttempts && !muteSuccess; attempt++)
                        {
                            muteSuccess = _microphoneManager.MuteMicrophone();

                            if (!muteSuccess)
                                await Task.Delay(100, cancellationToken); // Brief delay before retry
                        }

                        // If mute state changed, notify
                        bool isMuted = _microphoneManager.IsMicrophoneMuted();
                        if (wasMuted != isMuted)
                        {
                            OnMicrophoneMuteStateChanged(isMuted);
                            OnStatusChanged(isMuted ? "Microphone muted" : "Failed to mute microphone");
                        }
                    }

                    // Wait for the next check interval
                    await Task.Delay(_checkIntervalMs, cancellationToken);
                }
                catch (OperationCanceledException)
                {
                    break; // Exit on cancellation
                }
                catch (Exception ex)
                {
                    OnStatusChanged($"Error: {ex.Message}");
                    await Task.Delay(2000, cancellationToken); // Wait longer on error
                }
            }
        }

        public void ConfigureSettings(int checkIntervalMs, int initialDelayMs, int retryAttempts)
        {
            if (checkIntervalMs > 0)
                _checkIntervalMs = checkIntervalMs;

            if (initialDelayMs > 0)
                _initialDelayMs = initialDelayMs;

            if (retryAttempts > 0)
                _retryAttempts = retryAttempts;
        }

        private void OnStatusChanged(string status)
        {
            StatusChanged?.Invoke(this, status);
        }

        private void OnMicrophoneDetectionChanged(bool isDetected)
        {
            MicrophoneDetectionChanged?.Invoke(this, isDetected);
        }

        private void OnMicrophoneMuteStateChanged(bool isMuted)
        {
            MicrophoneMuteStateChanged?.Invoke(this, isMuted);
        }
    }
}