using Moose.Models;

using Plugin.BLE;
using Plugin.BLE.Abstractions.Contracts;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

using Xamarin.Essentials;

namespace Moose.Mobile.BLE
{
    public abstract class BLEAgent<TDevice, TDriver>
        where TDriver : Driver, new()
        where TDevice : SupportedBLEDevice<TDriver>
    {
        protected readonly IBluetoothLE ble;

        public string ServiceUuid { get; protected set; }
        public string ReadCharacteristicUuid { get; protected set; }
        public string WriteCharacteristicUuid { get; protected set; }

        public List<BLEDevice> Devices { get; private set; } = new List<BLEDevice>();
        public List<TDevice> SupportedDevices { get; private set; } = new List<TDevice>();

        public bool IsAvailable => ble.IsAvailable && ble.IsOn;
        public bool IsEnumerating => IsAvailable && ble.Adapter.IsScanning;

        public event EventHandler<BLEDevice> DeviceDiscovered;

        public event EventHandler<TDevice> SupportedDeviceDiscovered;
        public event EventHandler<TDevice> SupportedDeviceConnected;
        public event EventHandler<TDevice> SupportedDeviceConnectionLost;
        public event EventHandler<TDevice> SupportedDeviceDisconnected;

        public event EventHandler<IAdapter> EnumerationStarted;
        public event EventHandler<IAdapter> EnumerationStopped;
        public event EventHandler<IAdapter> StateChanged;

        public BLEAgent()
        {
            ble = CrossBluetoothLE.Current;
            ble.StateChanged += Ble_StateChanged;

            ble.Adapter.DeviceDiscovered += Adapter_DeviceDiscovered;
            ble.Adapter.DeviceConnected += Adapter_DeviceConnected;
            ble.Adapter.DeviceConnectionLost += Adapter_DeviceConnectionLost;
            ble.Adapter.DeviceDisconnected += Adapter_DeviceDisconnected;
        }

        public async Task<bool> RequestPermissionsAsync()
        {
            bool result = true;
            await Xamarin.Forms.Device.InvokeOnMainThreadAsync(async () =>
            {
                var status = await Permissions.RequestAsync<Permissions.LocationAlways>();
                if (status != PermissionStatus.Granted)
                    result = false;

            });
            return result;
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
            var device = SupportedDevices.FirstOrDefault(x => x.Id == e.Device.Id.ToString());
            if (device != null)
                SupportedDeviceDisconnected?.Invoke(this, device);
        }

        private void Adapter_DeviceConnectionLost(object sender, Plugin.BLE.Abstractions.EventArgs.DeviceErrorEventArgs e)
        {
            var device = SupportedDevices.FirstOrDefault(x => x.Id == e.Device.Id.ToString());
            if (device != null)
                SupportedDeviceConnectionLost?.Invoke(this, device);
        }

        private void Adapter_DeviceConnected(object sender, Plugin.BLE.Abstractions.EventArgs.DeviceEventArgs e)
        {
            var device = SupportedDevices.FirstOrDefault(x => x.Id == e.Device.Id.ToString());
            if (device != null)
                SupportedDeviceConnected?.Invoke(this, device);
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
                            var supportedDevice = CreateDevice(e.Device, service, read, write);
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

        public virtual async Task DisconnectAsync(TDevice device)
        {
            try
            {
                await device.ReleaseAsync();
                await ble.Adapter.DisconnectDeviceAsync(device.Device);
            }
            catch
            {
                //throw ex;
            }
        }

        protected virtual TDevice CreateDevice(IDevice device, IService service, ICharacteristic readCharacteristic, ICharacteristic writeCharacteristic)
        {
            throw new NotImplementedException("Please implement BLEAgent.CreateDevice");
        }
    }
}
