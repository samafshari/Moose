using MooseDrive.Services;

using RedCorners;
using RedCorners.Forms;
using RedCorners.Models;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Xamarin.Forms;
using MooseDrive.Models;

namespace MooseDrive.ViewModels
{
    public class MainViewModel : BindableModel
    {
        public IELMService ELMService { get; private set; }
        public List<DeviceViewModel> Items { get; set; } = new List<DeviceViewModel>();

        public MainViewModel()
        {
            Status = TaskStatuses.Success;
            ELMService = DependencyService.Get<IELMService>();
            ELMService.DiscoveredDevices.CollectionChanged += DiscoveredDevices_CollectionChanged;
        }

        private void DiscoveredDevices_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            Items = ELMService.DiscoveredDevices
                //.Where(x => x.Name.HasValue())
                .Select(x => new DeviceViewModel(new BLEDevice(x)))
                .ToList();

            UpdateProperties();
        }

        public override void OnStart()
        {
            base.OnStart();
            Refresh();
        }

        public override async void Refresh()
        {
            base.Refresh();
            if (!await ELMService.RequestPermissionsAsync())
            {
                App.Instance.DisplayAlert("Error", "Please grant the required permissions and try again.", "OK");
                return;
            }
            _ = Task.Run(FetchAsync);
        }

        async Task FetchAsync()
        {
            if (!await ELMService.IsBLEAvailableAsync())
            {
                App.Instance.DisplayAlert("Error", "Bluetooth is not available. Please check if it's on.", "OK");
                return;
            }

            Status = TaskStatuses.Busy;
            await ELMService.StartScanningAsync();
            Status = TaskStatuses.Success;
        }
    }
}
