using MooseDrive.Models;
using MooseDrive.Services.Implementations;

using Plugin.BLE.Abstractions.Contracts;


using RedCorners;
using RedCorners.Forms;

using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

using Xamarin.Forms;

namespace MooseDrive.ViewModels
{
    public class DeviceViewModel : BindableModel
    {
        public BLEDevice Device { get; private set; }

        public string Name { get; set; }
        public string Id { get; set; }
        public string Address { get; set; }

        public Action<DeviceViewModel> SelectAction;

        public DeviceViewModel()
        {
        }

        public DeviceViewModel(BLEDevice device) : this()
        {
            this.Device = device;

            Id = device.Device.Id.ToString();
            Name = device.Name;
            Address = device.Address;

            if (!Name.HasValue()) Name = "(No Name)";
        }

        public Command SelectCommand => new Command(() =>
        {
            SelectAction?.Invoke(this);
        });
    }
}
