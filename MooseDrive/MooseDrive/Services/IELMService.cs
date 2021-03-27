using Plugin.BLE.Abstractions.Contracts;

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Text;
using System.Threading.Tasks;

namespace MooseDrive.Services
{
    public interface IELMService
    {
        event EventHandler<bool> BluetoothStateChanged;
        event EventHandler<IDevice> DeviceConnected;
        event EventHandler<IDevice> DeviceDisconnected;

        ObservableCollection<IDevice> DiscoveredDevices { get; }
        IDevice ConnectedDevice { get; }

        Task<bool> RequestPermissionsAsync();
        Task<bool> IsBLEAvailableAsync();
        Task StartScanningAsync();
        Task ConnectAsync(IDevice device);
    }
}
