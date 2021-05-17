using System;
using System.Text;
using System.Linq;
using RedCorners.Forms;
using RedCorners.Models;
using System.Collections.Generic;
using Xamarin.Forms;
using RedCorners;
using MooseDrive.Models;
using Plugin.BLE.Abstractions.Contracts;

namespace MooseDrive.ViewModels
{
    public class CharacteristicViewModel : BindableModel
    {
        public BLEDevice Device { get; private set; }
        public IService Service { get; private set; }
        public ICharacteristic Characteristic { get; private set; }

        public CharacteristicViewModel()
        {
            Status = TaskStatuses.Success;
        }

        public CharacteristicViewModel(
            BLEDevice device, 
            IService service, 
            ICharacteristic characteristic) : this()
        {
            this.Device = device;
            this.Service = service;
            this.Characteristic = characteristic;
        }
    }
}
