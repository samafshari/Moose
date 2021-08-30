#if __IOS__
using CoreBluetooth;
#endif

using Plugin.BLE.Abstractions.Contracts;

using System;
using System.Collections.Generic;
using System.Text;

namespace MooseDrive.Models
{
    public class BLEDevice
    {
        public string Name { get; set; }
        public string Address { get; set; }
        public IDevice Device { get; set; }

        public BLEDevice(IDevice device)
        {
            this.Device = device;
            this.Name = device.Name;
#if __ANDROID__
            Address = (device as Plugin.BLE.Android.Device).BluetoothDevice.Address;
#elif __IOS__
            Address = ((device as Plugin.BLE.iOS.Device).NativeDevice as CBPeripheral).Identifier.ToString();
#endif
        }
    }
}
