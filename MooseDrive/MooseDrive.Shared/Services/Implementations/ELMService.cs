using Moose.ELM;

using MooseDrive.Services.Implementations;

using Plugin.BLE;
using Plugin.BLE.Abstractions;
using Plugin.BLE.Abstractions.Contracts;
using Plugin.BLE.Abstractions.EventArgs;
using Plugin.BLE.Abstractions.Exceptions;

using System;
using System.Text;
using System.Linq;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Collections.ObjectModel;

using Xamarin.Forms;
using Xamarin.Essentials;

using RedCorners.Forms;

[assembly: Dependency(typeof(ELMService))]
namespace MooseDrive.Services.Implementations
{
    public class ELMService : BindableModel, IELMService
    {
        readonly IBluetoothLE ble;

        ELMTerminal terminal = null;
        volatile bool isScanning = false;
        
        public event EventHandler<bool> BluetoothStateChanged;
        public event EventHandler<IDevice> DeviceConnected;
        public event EventHandler<IDevice> DeviceDisconnected;
        public ObservableCollection<IDevice> DiscoveredDevices { get; private set; } = new ObservableCollection<IDevice>();
        public IDevice ConnectedDevice { get; private set; }

        public ELMService()
        {
            ble = CrossBluetoothLE.Current;
            ble.StateChanged += Ble_StateChanged;
        }

        public async Task<bool> RequestPermissionsAsync()
        {
            var status = await Permissions.RequestAsync<Permissions.LocationAlways>();
            if (status != PermissionStatus.Granted)
            {
                return false;
            }
            return true;
        }

        public Task<bool> IsBLEAvailableAsync()
        {
            return Task.FromResult(ble.IsAvailable && ble.IsOn && ble.State == BluetoothState.On);
        }

        private void Ble_StateChanged(object sender, BluetoothStateChangedArgs e)
        {
            if (e.OldState == e.NewState) return;
            BluetoothStateChanged?.Invoke(this, ble.IsAvailable && ble.IsOn && ble.State == BluetoothState.On);
        }

        public async Task StartScanningAsync()
        {
            if (isScanning) return;
            DiscoveredDevices.Clear();
            try
            {
                isScanning = true;
                ble.Adapter.DeviceDiscovered += Adapter_DeviceDiscovered;
                ble.Adapter.ScanMode = ScanMode.LowLatency;
                await ble.Adapter.StartScanningForDevicesAsync();
            }
            catch
            {
                throw;
            }
            finally
            {
                ble.Adapter.DeviceDiscovered -= Adapter_DeviceDiscovered;
                isScanning = false;
            }
        }

        private void Adapter_DeviceDiscovered(object sender, DeviceEventArgs e)
        {
            if (!DiscoveredDevices.Any(x => x.Id == e.Device.Id))
                DiscoveredDevices.Add(e.Device);
        }

        public async Task ConnectAsync(IDevice device)
        {
            try
            {
                SubscribeAdapterEvents();
                await ble.Adapter.ConnectToDeviceAsync(device);
            }
            catch 
            {
                // ... could not connect to device
                throw;
            }
        }

        public async Task DisconnectAsync()
        {
            try
            {
                if (ConnectedDevice == null) return;
                await ble.Adapter.DisconnectDeviceAsync(ConnectedDevice);
                UnsubscribeAdapterEvents();
                ConnectedDevice = null;
            }
            catch
            {
                throw;
            }
        }

        void SubscribeAdapterEvents()
        {
            UnsubscribeAdapterEvents();
            ble.Adapter.DeviceDisconnected += Adapter_DeviceDisconnected;
            ble.Adapter.DeviceConnectionLost += Adapter_DeviceConnectionLost;
            ble.Adapter.DeviceConnected += Adapter_DeviceConnected;
        }

        void UnsubscribeAdapterEvents()
        {
            ble.Adapter.DeviceDisconnected -= Adapter_DeviceDisconnected;
            ble.Adapter.DeviceConnectionLost -= Adapter_DeviceConnectionLost;
            ble.Adapter.DeviceConnected -= Adapter_DeviceConnected;
        }

        private void Adapter_DeviceConnected(object sender, DeviceEventArgs e)
        {
            ConnectedDevice = e.Device;
            terminal = new ELMTerminal(e.Device);
            DeviceConnected?.Invoke(this, e.Device);
        }

        private void Adapter_DeviceConnectionLost(object sender, DeviceErrorEventArgs e)
        {
            HandleDisconnect();
        }

        private void Adapter_DeviceDisconnected(object sender, DeviceEventArgs e)
        {
            HandleDisconnect();
        }

        void HandleDisconnect()
        {
            if (ConnectedDevice == null) return;
            UnsubscribeAdapterEvents();
            DeviceDisconnected?.Invoke(this, ConnectedDevice);
            ConnectedDevice = null;

            if (terminal != null)
            {
                terminal.Disconnect();
                terminal = null;
            }
        }
    }
}
