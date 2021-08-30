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
using Xamarin.Essentials;
using Moose.ELM;

namespace MooseDrive.ViewModels
{
    public class MainViewModel : BindableModel
    {
        readonly IELMService elmService;
        readonly IErrorService errorService;

        public List<DeviceViewModel> Items { get; set; } = new List<DeviceViewModel>();

        public MainViewModel()
        {
            Status = TaskStatuses.Success;
            elmService = DependencyService.Get<IELMService>();
            elmService.DiscoveredDevices.CollectionChanged += DiscoveredDevices_CollectionChanged;

            errorService = DependencyService.Get<IErrorService>();
        }

        private void DiscoveredDevices_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            Items = elmService.DiscoveredDevices
                //.Where(x => x.Name.HasValue())
                .OrderBy(x => string.IsNullOrWhiteSpace(x.Name))
                .Select(x => new DeviceViewModel(new BLEDevice(x))
                {
                    SelectAction = SelectDevice
                })
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
            if (!await elmService.RequestPermissionsAsync())
            {
                var ask = await App.Instance.DisplayAlertAsync("Error", "Please grant the required permissions and try again.", "Settings", "Dismiss");
                if (ask)
                {
                    AppInfo.ShowSettingsUI();
                }
                return;
            }
            _ = Task.Run(FetchAsync);
        }

        async Task FetchAsync()
        {
            if (!await elmService.IsBLEAvailableAsync())
            {
                App.Instance.DisplayAlert("Error", "Bluetooth is not available. Please check if it's on.", "OK");
                return;
            }

            Status = TaskStatuses.Busy;
            await elmService.StartScanningAsync();
            Status = TaskStatuses.Success;
        }

        async void SelectDevice(DeviceViewModel device)
        {
            var terminal = await ELMTerminal.FromDeviceAsync(device?.Device?.Device);
            if (terminal == null)
            {
                errorService.AlertError("Device is incompatible.");
                return;
            }

            await App.Instance.ShowModalPageAsync(new Views.TerminalPage
            {
                BindingContext = new TerminalViewModel(terminal)
            });
        }
    }
}
