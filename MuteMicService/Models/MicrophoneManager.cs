using NAudio.CoreAudioApi;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MuteMicService.Models
{
    public class MicrophoneManager
    {
        private MMDeviceEnumerator _deviceEnumerator;
        private MMDevice _microphoneDevice;
        private bool _initialized = false;

        public MicrophoneManager()
        {
            _deviceEnumerator = new MMDeviceEnumerator();
        }

        public bool Initialize()
        {
            try
            {
                // Try to find the active microphone device
                _microphoneDevice = GetActiveMicrophone();
                _initialized = _microphoneDevice != null;
                return _initialized;
            }
            catch (Exception)
            {
                _initialized = false;
                return false;
            }
        }

        public bool IsMicrophoneAvailable()
        {
            return _initialized && _microphoneDevice != null;
        }

        public bool IsMicrophoneMuted()
        {
            if (!IsMicrophoneAvailable())
                return false;

            try
            {
                return _microphoneDevice.AudioEndpointVolume.Mute;
            }
            catch
            {
                return false;
            }
        }

        public bool MuteMicrophone()
        {
            if (!IsMicrophoneAvailable())
                return false;

            try
            {
                _microphoneDevice.AudioEndpointVolume.Mute = true;
                return true;
            }
            catch
            {
                return false;
            }
        }

        public bool UnmuteMicrophone()
        {
            if (!IsMicrophoneAvailable())
                return false;

            try
            {
                _microphoneDevice.AudioEndpointVolume.Mute = false;
                return true;
            }
            catch
            {
                return false;
            }
        }

        public string GetCurrentMicrophoneName()
        {
            if (!IsMicrophoneAvailable())
                return "No microphone available";

            return _microphoneDevice.FriendlyName;
        }

        public List<string> GetAvailableMicrophones()
        {
            var microphones = new List<string>();

            try
            {
                var captureDevices = _deviceEnumerator.EnumerateAudioEndPoints(DataFlow.Capture, DeviceState.Active);
                foreach (var device in captureDevices)
                {
                    microphones.Add(device.FriendlyName);
                    device.Dispose();
                }
            }
            catch { }

            return microphones;
        }

        private MMDevice GetActiveMicrophone()
        {
            try
            {
                // Get the default capture device
                var defaultDevice = _deviceEnumerator.GetDefaultAudioEndpoint(DataFlow.Capture, Role.Communications);
                if (defaultDevice != null)
                    return defaultDevice;

                // If no default device, try to get any active capture device
                var captureDevices = _deviceEnumerator.EnumerateAudioEndPoints(DataFlow.Capture, DeviceState.Active);
                return captureDevices.FirstOrDefault();
            }
            catch
            {
                return null;
            }
        }

        public void Refresh()
        {
            try
            {
                _microphoneDevice?.Dispose();
                _microphoneDevice = GetActiveMicrophone();
                _initialized = _microphoneDevice != null;
            }
            catch
            {
                _initialized = false;
            }
        }

        public void Dispose()
        {
            _microphoneDevice?.Dispose();
            _deviceEnumerator?.Dispose();
        }
    }
}