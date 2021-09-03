using Moose.Mobile.BLE;
using Moose.Mobile.Services.Implementations;
using Moose.Models;

using MooseDrive;
using MooseDrive.Mobile.App;
using MooseDrive.Mobile.App.Services;

using Plugin.BLE.Abstractions.Contracts;

using RedCorners.Forms;

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Text;
using System.Threading.Tasks;

using Xamarin.Forms;

[assembly: Dependency(typeof(BluetoothService))]
namespace Moose.Mobile.Services.Implementations
{
    public class BluetoothService : BindableModel, IBluetoothService
    {
        readonly ISettingsService settingsService;
        string autoConnectQuery;

        public event EventHandler<ELMDevice> Connected;
        public event EventHandler<ELMDevice> Disconnected;
        public event EventHandler<ELMDriver> OnUpdate;

        public ELMAgent Agent { get; }
        public ObservableCollection<ELMDevice> DiscoveredDevices { get; }
        public ELMDevice Device { get; private set; }

        volatile bool _isEnumerating = false;
        public bool IsEnumerating
        {
            get => _isEnumerating;
            set
            {
                if (IsEnumerating == value) return;
                _isEnumerating = value;
                RaisePropertyChanged();
            }
        }

        bool _isConnected = false;
        public bool IsConnected
        {
            get => _isConnected;
            set => SetProperty(ref _isConnected, value);
        }

        public BluetoothService()
        {
            RaisePropertyChangeOnUI = true;
            DiscoveredDevices = new ObservableCollection<ELMDevice>();
            Agent = new ELMAgent();

            Agent.EnumerationStarted += Agent_EnumerationStarted;
            Agent.EnumerationStopped += Agent_EnumerationStopped;
            Agent.SupportedDeviceDiscovered += Agent_SupportedDeviceDiscovered;
            Agent.SupportedDeviceDisconnected += Agent_SupportedDeviceDisconnected;
            Agent.DeviceDiscovered += Agent_DeviceDiscovered;

            settingsService = DependencyService.Get<ISettingsService>();
            BeginAutoConnect();
        }

        void Invoke(Action a) => Xamarin.Forms.Device.BeginInvokeOnMainThread(a);

        private async void Agent_SupportedDeviceDisconnected(object sender, ELMDevice device)
        {
            await DisconnectAsync();
        }

        private void Agent_EnumerationStarted(object sender, IAdapter result)
        {
            IsEnumerating = true;
        }

        private void Agent_EnumerationStopped(object sender, IAdapter result)
        {
            IsEnumerating = false;
        }

        public async Task StartEnumeratingDevicesAsync(string autoConnectQuery)
        {
            if (!await Agent.RequestPermissionsAsync())
                throw new ApplicationException("Permissions are not granted.");

            if (IsEnumerating) return;
            IsEnumerating = true;
            try
            {
                Invoke(DiscoveredDevices.Clear);
                this.autoConnectQuery = autoConnectQuery;
                await Agent.StartEnumeratingDevicesAsync(false);
            }
            catch (Exception ex)
            {
                throw ex;
            }
            finally
            {
                IsEnumerating = false;
            }
        }

        public async Task StopEnumeratingDevicesAsync()
        {
            if (!IsEnumerating) return;
            try
            {
                await Agent.StopEnumeratingDevicesAsync();
            }
            catch (Exception ex)
            {
                throw ex;
            }
            finally
            {
                IsEnumerating = false;
            }
        }

        private void Agent_SupportedDeviceDiscovered(object sender, ELMDevice o2)
        {
            Invoke(() => DiscoveredDevices.Add(o2));
        }

        private void Agent_DeviceDiscovered(object sender, BLEDevice device)
        {
            if (device is ELMDevice elmDevice)
            {
                if (!string.IsNullOrWhiteSpace(autoConnectQuery))
                {
                    if (elmDevice.Device.Name.Contains(autoConnectQuery) || elmDevice.Device.Id.ToString().Contains(autoConnectQuery))
                    {
                        Device = elmDevice;
                        Xamarin.Forms.Device.BeginInvokeOnMainThread(async () =>
                        {
                            await StopEnumeratingDevicesAsync();
                            await ConnectAsync(Device);
                        });
                    }
                }
            }
        }

        public async Task ConnectAsync(ELMDevice device)
        {
            if (IsConnected) return;
            try
            {
                if (await Agent.ConnectAsync(device))
                {
                    device.LogPublished += Device_LogPublished;
                    IsConnected = true;
                    device.Driver.OnUpdate += (s, e) => OnUpdate(this, s as ELMDriver);
                    Device = device;
                    await device.Driver.InitializeAsync();
                    Connected?.Invoke(this, device);
                    _ = Task.Run(ReadRealtimeAsync);
                    settingsService.Settings.LastDeviceName = device.Name;
                    settingsService.Save();
                }
            }
            catch (Exception ex)
            {
                App.Instance.DisplayAlert("Error Connecting", ex.ToString(), "OK");
            }
        }

        private void Device_LogPublished(object sender, string result)
        {
#if DEBUG
            Console.WriteLine($"Log: {result}");
#endif
        }

        public async Task DisconnectAsync()
        {
            if (!IsConnected) return;
            try
            {
                IsConnected = false;
                await Device.Driver.DisconnectAsync();
                await Agent.DisconnectAsync(Device);
                settingsService.Settings.LastDeviceName = null;
                settingsService.Save();
            }
            catch (Exception ex)
            {
                throw ex;
            }
            finally
            {
                Disconnected?.Invoke(this, Device);
                Device = null;
                UpdateProperties();
            }
        }

        void ReadRealtimeAsync()
        {
            Task.Run(async () =>
            {
                while (IsConnected && Device?.Driver != null)
                {
                    try
                    {
                        await Device.Driver.UpdateAsync();
                    }
                    catch   (Exception ex)
                    {
                        Console.WriteLine($"Error in read realtime: {ex}");
                    }
                }
                Console.WriteLine("Realtime read finished");
            });
        }

        void BeginAutoConnect()
        {
            if (!settingsService.Settings.AutoConnect ||
                string.IsNullOrWhiteSpace(settingsService.Settings.LastDeviceName))
                return;

            Task.Run(() => StartEnumeratingDevicesAsync(settingsService.Settings.LastDeviceName));
        }
    }
}
