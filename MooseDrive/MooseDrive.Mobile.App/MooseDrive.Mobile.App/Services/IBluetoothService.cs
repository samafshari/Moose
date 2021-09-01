using Moose.Mobile.BLE;
using Moose.Models;

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Text;
using System.Threading.Tasks;

namespace MooseDrive.Mobile.App.Services
{
    public interface IBluetoothService : INotifyPropertyChanged 
    {
        bool IsEnumerating { get; }
        bool IsConnected { get; }

        ELMDevice Device { get; }
        ELMAgent Agent { get; }
        
        event EventHandler<ELMDevice> Connected;
        event EventHandler<ELMDevice> Disconnected;
        event EventHandler<ELMDriver> OnUpdate;

        ObservableCollection<ELMDevice> DiscoveredDevices { get; }

        Task StartEnumeratingDevicesAsync(string autoConnectQuery);
        Task StopEnumeratingDevicesAsync();
        Task ConnectAsync(ELMDevice device);
        Task DisconnectAsync();
    }
}
