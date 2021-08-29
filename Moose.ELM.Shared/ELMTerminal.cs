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
        public IService WriteService { get; private set; }
        public ICharacteristic WriteCharacteristic { get; private set; }
        public List<ICharacteristic> Characteristics { get; private set; }
        
        public event EventHandler<string> OnMessage;
        public event EventHandler<string> OnMessageFromWriteCharacteristic;

        ELMTerminal(IDevice device, IService service, ICharacteristic characteristic)
        {
            ble = CrossBluetoothLE.Current;
            this.Device = device;
            this.WriteService = service;
            this.WriteCharacteristic = characteristic;
            WriteCharacteristic.ValueUpdated += Characteristic_ValueUpdated;
            SubscribeAdapterEvents();
        }

        public async Task DisconnectAsync()
        {
            foreach (var characteristic in Characteristics.Where(x => x.CanUpdate))
            {
                await characteristic.StopUpdatesAsync();
            }
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
                var writeService = services.FirstOrDefault(x => x.Id.ToString().ToUpper() == ServiceId);
                if (writeService != null)
                {
                    var characteristics = await writeService.GetCharacteristicsAsync();
                    var writeCharacteristic = characteristics?.FirstOrDefault(x => x.Uuid.ToUpper() == CharacteristicUuid);
                    if (writeCharacteristic != null)
                    {
                        var terminal = new ELMTerminal(device, writeService, writeCharacteristic);
                        await terminal.InitializeAsync();
                        return terminal;
                    }
                }

                await ble.Adapter.DisconnectDeviceAsync(device);
                return null;
            }
            finally
            {

            }
        }

        async Task InitializeAsync()
        {
            //WriteCharacteristic.ValueUpdated += Characteristic_ValueUpdated;
            //await WriteCharacteristic.StartUpdatesAsync();

            Characteristics = new List<ICharacteristic>();
            var services = await Device.GetServicesAsync();
            foreach (var service in services)
            {
                var characteristics = await service.GetCharacteristicsAsync();
                foreach (var characteristic in characteristics)
                {
                    Console.WriteLine(
                        $"Service: {service.Id}, {service.IsPrimary}; " +
                        $"Characteristic: {characteristic.Uuid}; " +
                        $"{characteristic.CanRead}, {characteristic.CanUpdate}, {characteristic.CanWrite}; " +
                        $"{characteristic.Properties}, {characteristic.WriteType}");
                    characteristic.ValueUpdated += Characteristic_ValueUpdated;
                    if (characteristic.CanUpdate)
                        await characteristic.StartUpdatesAsync();
                    Characteristics.Add(characteristic);
                }
            }
        }

        public async Task<bool> WriteAsync(string message)
        {
            var bytes = Encoding.ASCII.GetBytes(message);
            Console.WriteLine($"Writing: {message}");
            try
            {
                await WriteCharacteristic.WriteAsync(bytes);
                return true;
            }
            catch
            {
                return false;
            }
        }

        private async void Characteristic_ValueUpdated(object sender, CharacteristicUpdatedEventArgs e)
        {
            await ReadAsync(e.Characteristic);
        }

        async Task<string> ReadAsync(ICharacteristic characteristic)
        {
            try
            {
                //var bytes = await characteristic.ReadAsync();
                //if (bytes == null)
                //{
                //    Console.WriteLine($"[{characteristic.Uuid}] Read null");
                //    return null;
                //}
                var message = characteristic.StringValue; // Encoding.ASCII.GetString(bytes);
                //Console.WriteLine($"[{characteristic.Uuid}] Read: {message}");
                OnMessage?.Invoke(characteristic, message);
                if (characteristic == WriteCharacteristic)
                    OnMessageFromWriteCharacteristic?.Invoke(characteristic, message);
                return message;
            }
            catch
            {
                Console.WriteLine($"[{characteristic.Uuid}] Read Error");
                return null;
            }
        }


        public static string ToHexString(byte[] bytes)
        {
            if (bytes == null) return null;
            StringBuilder hex = new StringBuilder(bytes.Length * 2);
            foreach (byte b in bytes)
                hex.AppendFormat("{0:x2}", b);
            return hex.ToString();
        }
    }
}
