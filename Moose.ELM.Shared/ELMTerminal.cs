using Plugin.BLE;
using Plugin.BLE.Abstractions.Contracts;
using Plugin.BLE.Abstractions.EventArgs;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Moose.ELM
{
    public class ELMTerminal
    {
        readonly IBluetoothLE ble;

        public const string ServiceId = "E7810A71-73AE-499D-8C15-FAA9AEF0C3F2";
        public const string CharacteristicUuid = "BEF8D6C9-9C21-4C9E-B632-BD58C1009F9F";

        public IDevice Device { get; private set; }
        public IService Service { get; private set; }
        public ICharacteristic Characteristic { get; private set; }

        ELMTerminal(IDevice device, IService service, ICharacteristic characteristic)
        {
            ble = CrossBluetoothLE.Current;
            this.Device = device;
            this.Service = service;
            this.Characteristic = characteristic;
            Characteristic.ValueUpdated += Characteristic_ValueUpdated;
            SubscribeAdapterEvents();
            Task.Run(Characteristic.StartUpdatesAsync);
        }

        public async Task DisconnectAsync()
        {
            await Characteristic.StopUpdatesAsync();
            UnsubscribeAdapterEvents();
            await ble.Adapter.DisconnectDeviceAsync(Device);
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

        void Adapter_DeviceConnected(object sender, DeviceEventArgs e)
        {
        }

        async void Adapter_DeviceConnectionLost(object sender, DeviceErrorEventArgs e)
        {
            await Task.Delay(100);
            await ble.Adapter.ConnectToDeviceAsync(Device);
        }

        async void Adapter_DeviceDisconnected(object sender, DeviceEventArgs e)
        {
            await Task.Delay(100);
            await ble.Adapter.ConnectToDeviceAsync(Device);
        }

        public static async Task<ELMTerminal> FromDeviceAsync(IDevice device)
        {
            if (device == null) return null;
            try
            {
                var ble = CrossBluetoothLE.Current;
                await ble.Adapter.ConnectToDeviceAsync(device);

                var services = await device.GetServicesAsync();
                var service = services.FirstOrDefault(x => x.Id.ToString().ToUpper() == ServiceId);
                if (service != null)
                {
                    var characteristics = await service.GetCharacteristicsAsync();
                    var characteristic = characteristics?.FirstOrDefault(x => x.Uuid.ToUpper() == CharacteristicUuid);
                    if (characteristic != null)
                    {
                        return new ELMTerminal(device, service, characteristic);
                    }
                }

                await ble.Adapter.DisconnectDeviceAsync(device);
                return null;
            }
            finally
            {

            }
        }

        public async Task<string> WriteAsync(string message)
        {
            var bytes = Encoding.ASCII.GetBytes(message);
            Console.WriteLine($"Writing: {message}");
            await Characteristic.WriteAsync(bytes);
            return await ReadAsync();
        }

        async Task<string> ReadAsync()
        {
            var bytes = await Characteristic.ReadAsync();
            if (bytes == null)
            {
                Console.WriteLine($"Read null");
                return null;
            }
            var message = Encoding.ASCII.GetString(bytes);
            Console.WriteLine($"Read: {message}");
            return message;
        }

        private async void Characteristic_ValueUpdated(object sender, CharacteristicUpdatedEventArgs e)
        {
            await ReadAsync();
        }
    }
}
