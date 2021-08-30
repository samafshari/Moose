using Plugin.BLE.Abstractions.Contracts;


using RedCorners;
using RedCorners.Forms;

using System;
using System.Collections.Generic;
using System.Text;

namespace MooseDrive.ViewModels
{
    public class DeviceViewModel : BindableModel
    {
        IDevice device;

        public string Name { get; set; }
        public string Id { get; set; }

        public DeviceViewModel(IDevice device)
        {
            this.device = device;
            Name = device.Name;
            if (!Name.HasValue()) Name = "(No Name)";
            Id = device.Id.ToString();
        }
    }
}
