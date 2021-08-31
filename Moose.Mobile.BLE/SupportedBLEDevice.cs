using Moose.Models;

using Plugin.BLE.Abstractions.Contracts;
using Plugin.BLE.Abstractions.EventArgs;

using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Moose.Mobile.BLE
{
    public class SupportedBLEDevice : BLEDevice
    {
        protected IService service;
        protected ICharacteristic readCharacteristic;
        protected ICharacteristic writeCharacteristic;

        public IDevice Device { get; private set; }

        public event EventHandler<string> LogPublished;

        public bool IsReleased { get; private set; }

        public SupportedBLEDevice(
            IDevice device,
            IService service,
            ICharacteristic readCharacteristic,
            ICharacteristic writeCharacteristic) :
            base(device.Id.ToString(), device.Name)
        {
            this.Device = device;
            this.service = service;
            this.readCharacteristic = readCharacteristic;
            this.writeCharacteristic = writeCharacteristic;
            this.IsSupported = true;
        }

        public async Task SetupAsync(IService service, ICharacteristic readCharacteristic, ICharacteristic writeCharacteristic)
        {
            this.service = service;
            this.readCharacteristic = readCharacteristic;
            this.writeCharacteristic = writeCharacteristic;
            if (this.readCharacteristic != null)
                this.readCharacteristic.ValueUpdated -= ReadCharacteristic_ValueUpdated;
            await SetupAsync();
        }

        public virtual async Task SetupAsync()
        {
            readCharacteristic.ValueUpdated += ReadCharacteristic_ValueUpdated;
            await readCharacteristic.StartUpdatesAsync();
        }

        void ReadCharacteristic_ValueUpdated(object sender, Plugin.BLE.Abstractions.EventArgs.CharacteristicUpdatedEventArgs e)
        {
            Update(e);
        }

        protected virtual void Update(CharacteristicUpdatedEventArgs e)
        {
        }

        public virtual Task ReleaseAsync()
        {
            if (!IsReleased)
            {
                IsReleased = true;
                readCharacteristic.ValueUpdated -= ReadCharacteristic_ValueUpdated;
                //await readCharacteristic.StopUpdatesAsync();
                //service.Dispose();
                //Device.Dispose();
            }
            return Task.CompletedTask;
        }

        public Task LogAsync(string message)
        {
            LogPublished?.Invoke(this, message);
            return Task.CompletedTask;
        }
    }

    public class SupportedBLEDevice<TDriver> : SupportedBLEDevice where TDriver : Driver, new()
    {
        public TDriver Driver { get; private set; }

        public SupportedBLEDevice(IDevice device,
            IService service,
            ICharacteristic readCharacteristic,
            ICharacteristic writeCharacteristic) : 
            base(device, service, readCharacteristic, writeCharacteristic)
        {
        }

        public override async Task SetupAsync()
        {
            await base.SetupAsync();
            Driver = new TDriver();
            Driver.WriteAsyncFunc = WriteAsync;
            Driver.LogAsyncFunc = LogAsync;
            await Driver.SetupAsync();
        }

        protected override void Update(CharacteristicUpdatedEventArgs e)
        {
            Driver?.InjectMessage(e.Characteristic.Value);
            Driver?.InjectMessage(e.Characteristic.StringValue);
        }

        private void ReadCharacteristic_ValueUpdated(object sender, Plugin.BLE.Abstractions.EventArgs.CharacteristicUpdatedEventArgs e)
        {
            Driver?.InjectMessage(e.Characteristic.Value);
            Driver?.InjectMessage(e.Characteristic.StringValue);
        }

        public async Task WriteAsync(byte[] bytes)
        {
            await writeCharacteristic.WriteAsync(bytes);
        }
    }
}
