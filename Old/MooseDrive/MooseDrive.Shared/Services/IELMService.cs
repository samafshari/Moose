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

        ObservableCollection<IDevice> DiscoveredDevices { get; }

        Task<bool> RequestPermissionsAsync();
        Task<bool> IsBLEAvailableAsync();
        Task StartScanningAsync();
    }
}
