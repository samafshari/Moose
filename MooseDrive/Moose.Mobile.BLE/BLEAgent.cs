using Moose.Models;

using Plugin.BLE;
using Plugin.BLE.Abstractions.Contracts;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Moose.Mobile.BLE
{
    public abstract class BLEAgent<TDevice, TDriver> where TDriver : Driver, new() where TDevice : SupportedBLEDevice<TDriver>
    {
        readonly IBluetoothLE ble;

        public string ServiceUuid { get; protected set; }
        public string ReadCharacteristicUuid { get; protected set; }
        public string WriteCharacteristicUuid { get; protected set; }

        public List<BLEDevice> Devices { get; private set; } = new List<BLEDevice>();
        public List<TDevice> SupportedDevices { get; private set; } = new List<TDevice>();

        public bool IsAvailable => ble.IsAvailable && ble.IsOn;
        public bool IsEnumerating => IsAvailable && ble.Adapter.IsScanning;

        public event TypedEventHandler<BLEAgent<TDevice, TDriver>, BLEDevice> DeviceDiscovered;

        public event TypedEventHandler<BLEAgent<TDevice, TDriver>, TDevice> SupportedDeviceDiscovered;
        public event TypedEventHandler<BLEAgent<TDevice, TDriver>, TDevice> SupportedDeviceConnected;
        public event TypedEventHandler<BLEAgent<TDevice, TDriver>, TDevice> SupportedDeviceConnectionLost;
        public event TypedEventHandler<BLEAgent<TDevice, TDriver>, TDevice> SupportedDeviceDisconnected;

        public event TypedEventHandler<BLEAgent<TDevice, TDriver>, IAdapter> EnumerationStarted;
        public event TypedEventHandler<BLEAgent<TDevice, TDriver>, IAdapter> EnumerationStopped;
        public event TypedEventHandler<BLEAgent<TDevice, TDriver>, IAdapter> StateChanged;

        public BLEAgent()
        {
            ble = CrossBluetoothLE.Current;
            ble.StateChanged += Ble_StateChanged;

            ble.Adapter.DeviceDiscovered += Adapter_DeviceDiscovered;
            ble.Adapter.DeviceConnected += Adapter_DeviceConnected;
            ble.Adapter.DeviceConnectionLost += Adapter_DeviceConnectionLost;
            ble.Adapter.DeviceDisconnected += Adapter_DeviceDisconnected;
        }

        private void Ble_StateChanged(object sender, Plugin.BLE.Abstractions.EventArgs.BluetoothStateChangedArgs e)
        {
            StateChanged?.Invoke(this, ble.Adapter);
        }
        public async Task StartEnumeratingDevicesAsync(bool knownOnly)
        {
            if (IsEnumerating) return;
            try
            {
                Guid[] uuids = null;
                if (knownOnly) uuids = new Guid[] { new Guid(ServiceUuid) };
                // ble.Adapter.ScanMode = ScanMode.LowLatency;
                EnumerationStarted?.Invoke(this, ble.Adapter);
                await ble.Adapter.StartScanningForDevicesAsync(uuids);
                EnumerationStopped?.Invoke(this, ble.Adapter);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public async Task StopEnumeratingDevicesAsync()
        {
            if (!IsEnumerating) return;
            await ble.Adapter.StopScanningForDevicesAsync();
        }

        private void Adapter_DeviceDisconnected(object sender, Plugin.BLE.Abstractions.EventArgs.DeviceEventArgs e)
        {
            var o2device = SupportedDevices.FirstOrDefault(x => x.Id == e.Device.Id.ToString());
            if (o2device != null)
                SupportedDeviceDisconnected?.Invoke(this, o2device);
        }

        private void Adapter_DeviceConnectionLost(object sender, Plugin.BLE.Abstractions.EventArgs.DeviceErrorEventArgs e)
        {
            var o2device = SupportedDevices.FirstOrDefault(x => x.Id == e.Device.Id.ToString());
            if (o2device != null)
                SupportedDeviceConnectionLost?.Invoke(this, o2device);
        }

        private void Adapter_DeviceConnected(object sender, Plugin.BLE.Abstractions.EventArgs.DeviceEventArgs e)
        {
            var o2device = SupportedDevices.FirstOrDefault(x => x.Id == e.Device.Id.ToString());
            if (o2device != null)
                SupportedDeviceConnected?.Invoke(this, o2device);
        }

        private async void Adapter_DeviceDiscovered(object sender, Plugin.BLE.Abstractions.EventArgs.DeviceEventArgs e)
        {
            Console.WriteLine($"Device: {e.Device.Name}");
            if (e.Device.Name == null) return;
            BLEDevice bleDevice = null;

            try
            {
                Console.WriteLine("AAAAAAAAAAAAAA");
                await ble.Adapter.ConnectToDeviceAsync(e.Device);
                Console.WriteLine("BBBBBBBBBBBB");
                var services = await e.Device.GetServicesAsync();
                Console.WriteLine("CCCCCCCCCCCCCCC");
                var service = await e.Device.GetServiceAsync(new Guid(ServiceUuid));
                Console.WriteLine("DDDDDDDDDDDDDDDD");
                if (service != null)
                {
                    var read = await service.GetCharacteristicAsync(new Guid(ReadCharacteristicUuid));
                    Console.WriteLine("EEEEEEEEEEEEEEEE");
                    if (read != null)
                    {
                        var write = await service.GetCharacteristicAsync(new Guid(WriteCharacteristicUuid));
                        Console.WriteLine("FFFFFFFFFFFFFFFF");
                        if (write != null)
                        {
                            // Device is supported
                            Console.WriteLine("GGGGGGGGGGGGG");
                            var supportedDevice = new TDevice(e.Device, service, read, write);
                            await supportedDevice.SetupAsync();
                            SupportedDevices.Add(supportedDevice);
                            SupportedDeviceDiscovered?.Invoke(this, supportedDevice);
                            bleDevice = supportedDevice;
                            Console.WriteLine("HHHHHHHHHHHH");
                        }
                    }
                }
                Console.WriteLine("IIIIIIIIIII");

                if (bleDevice == null)
                {
                    bleDevice = new BLEDevice(e.Device.Id.ToString(), e.Device.Name);
                }

                Devices.Add(bleDevice);
                await ble.Adapter.DisconnectDeviceAsync(e.Device);
                DeviceDiscovered?.Invoke(this, bleDevice);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error for device {e.Device.Name}: {ex}");
            }
        }

        public async Task<bool> ConnectAsync(TDevice device)
        {
            await ble.Adapter.ConnectToDeviceAsync(device.Device);
            var service = await device.Device.GetServiceAsync(new Guid(ServiceUuid));
            if (service != null)
            {
                var read = await service.GetCharacteristicAsync(new Guid(ReadCharacteristicUuid));
                if (read != null)
                {
                    var write = await service.GetCharacteristicAsync(new Guid(WriteCharacteristicUuid));
                    if (write != null)
                    {
                        await device.SetupAsync(service, read, write);
                        return true;
                    }
                }
            }
            return false;
        }

        public async Task DisconnectAsync(TDevice device)
        {
            try
            {
                await device.ReleaseAsync();
                await ble.Adapter.DisconnectDeviceAsync(device.Device);
            }
            catch (Exception ex)
            {
                //throw ex;
            }
        }
    }
}
