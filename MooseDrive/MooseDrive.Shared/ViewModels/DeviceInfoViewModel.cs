using System;
using System.Text;
using System.Linq;
using RedCorners.Forms;
using RedCorners.Models;
using System.Collections.Generic;
using Xamarin.Forms;
using RedCorners;
using MooseDrive.Models;
using MooseDrive.Services.Implementations;
using System.Threading.Tasks;
using Moose.ELM;

namespace MooseDrive.ViewModels
{
    public class DeviceInfoViewModel : BindableModel
    {
        readonly BLEDevice Device;
        readonly ELMService elmService;

        public string Name { get; set; }
        public string Id { get; set; }
        public bool IsSupported { get; set; }

        bool _isConnected = false;
        public bool IsConnected
        {
            get => _isConnected;
            set => SetProperty(ref _isConnected, value);
        }

        volatile bool _isConnecting = false;
        public bool IsConnecting
        {
            get => _isConnecting;
            set
            {
                _isConnecting = value;
                RaisePropertyChanged();
            }
        }

        List<ServiceViewModel> _services = new List<ServiceViewModel>();
        public List<ServiceViewModel> Services
        {
            get => _services;
            set => SetProperty(ref _services, value);
        }

        public DeviceInfoViewModel()
        {
            elmService = DependencyService.Get<ELMService>();
        }

        public DeviceInfoViewModel(BLEDevice device) : this()
        {
            this.Device = device;
            Name = device.Name;
            if (!Name.HasValue()) Name = "(No Name)";
            Id = device.Device.Id.ToString();
        }

        public override void OnStart()
        {
            base.OnStart();
            Refresh();
            elmService.DeviceConnected += ElmService_DeviceConnected;
            elmService.DeviceDisconnected += ElmService_DeviceDisconnected;
        }

        public override async void OnStop()
        {
            if (IsConnected) await elmService.DisconnectAsync();
            elmService.DeviceConnected -= ElmService_DeviceConnected;
            elmService.DeviceDisconnected -= ElmService_DeviceDisconnected;
            base.OnStop();
        }

        public override async void Refresh()
        {
            base.Refresh();
            if (!IsConnected)
            {
                await ConnectAsync();
                return;
            }
            await FetchServicesAsync();
            Status = TaskStatuses.Success;
        }

        async Task ConnectAsync()
        {
            if (IsConnected || IsConnecting) return;
            IsConnecting = true;
            await elmService.ConnectAsync(Device.Device);
            IsConnecting = false;
        }

        private void ElmService_DeviceConnected(object sender, Plugin.BLE.Abstractions.Contracts.IDevice e)
        {
            IsConnected = true;
            Refresh();
        }

        private void ElmService_DeviceDisconnected(object sender, Plugin.BLE.Abstractions.Contracts.IDevice e)
        {
            IsConnected = false;
        }

        async Task FetchServicesAsync()
        {
            var services = await Device.Device.GetServicesAsync();
            if (services != null)
            {
                Services = services.Select(x => new ServiceViewModel(Device, x)).ToList();
                foreach (var item in Services)
                {
                    await item.FetchCharacteristicsAsync();

                    if (item.Id.ToUpper() == ELMTerminal.ServiceId)
                    {
                        if (item.Characteristics.Any(x => x.Characteristic.Uuid.ToString() == ELMTerminal.CharacteristicUuid))
                        {
                            IsSupported = true;
                        }
                    }
                }
            }
        }
    }
}
