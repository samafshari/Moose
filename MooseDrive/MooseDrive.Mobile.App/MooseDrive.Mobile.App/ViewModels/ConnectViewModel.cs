using MooseDrive.Mobile.App.Services;

using RedCorners;
using RedCorners.Forms;
using RedCorners.Models;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Xamarin.Forms;

namespace MooseDrive.Mobile.App.ViewModels
{
    public class ConnectViewModel : BindableModel
    {
        public IBluetoothService Service { get; }
        public List<DeviceViewModel> Items { get; private set; }

        public bool IsEmpty { get; set; }

        public ConnectViewModel()
        {
            Service = DependencyService.Get<IBluetoothService>();
            Items = new List<DeviceViewModel>();
            Status = TaskStatuses.Success;
            Refresh();
        }

        public override void OnStart()
        {
            base.OnStart();
            Service.PropertyChanged += Service_PropertyChanged;
            Service.DiscoveredDevices.CollectionChanged += DiscoveredDevices_CollectionChanged;
            Service.Agent.SupportedDeviceConnected += Agent_SupportedDeviceConnected;
            Device.BeginInvokeOnMainThread(() =>
            {
                Items = Service.DiscoveredDevices.Select(x => new DeviceViewModel(x)).ToList();
                RaisePropertyChanged(nameof(Items));
            });
        }

        public override void OnStop()
        {
            Service.Agent.SupportedDeviceConnected -= Agent_SupportedDeviceConnected;
            Service.DiscoveredDevices.CollectionChanged -= DiscoveredDevices_CollectionChanged;
            Service.PropertyChanged -= Service_PropertyChanged;
            base.OnStop();
        }

        public override async Task RefreshAsync()
        {
            Status = TaskStatuses.Busy;
            await Service.StartEnumeratingDevicesAsync(null);
        }

        private void DiscoveredDevices_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            Device.BeginInvokeOnMainThread(() =>
            {
                Items = Service.DiscoveredDevices.Select(x => new DeviceViewModel(x)).ToList();
                RaisePropertyChanged(nameof(Items));
            });
        }

        private void Service_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            if (!Service.IsEnumerating) Status = TaskStatuses.Success;
            IsEmpty = Items.Count == 0 && !Service.IsEnumerating;
            RaisePropertyChanged(nameof(IsEmpty));
        }

        private void Agent_SupportedDeviceConnected(object sender, ELMDevice result)
        {
            GoBackCommand.Execute(null);
        }
    }
}
