using MooseDrive.Mobile.App.Services;

using RedCorners;
using RedCorners.Forms;
using RedCorners.Models;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Xamarin.Forms;

namespace MooseDrive.Mobile.App.ViewModels
{
    public class DeviceViewModel : BindableModel
    {
        public IBluetoothService Service { get; }
        public ELMDevice Device { get; private set; }

        public DeviceViewModel(ELMDevice device)
        {
            Service = DependencyService.Get<IBluetoothService>();
            this.Device = device;
            Status = TaskStatuses.Success;
        }

        public Command ConnectCommand => new Command(async () =>
        {
            await Service.ConnectAsync(Device);
        });
    }
}
