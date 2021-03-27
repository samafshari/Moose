using MooseDrive.Models;

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
        readonly BLEDevice device;

        public string Name { get; set; }
        public string Id { get; set; }

        public DeviceViewModel(BLEDevice device)
        {
            this.device = device;
            Name = device.Name;
            if (!Name.HasValue()) Name = "(No Name)";
            Id = device.Device.Id.ToString();
        }
    }
}
