using MooseDrive.Models;

using Plugin.BLE.Abstractions.Contracts;


using RedCorners;
using RedCorners.Forms;
using RedCorners.Models;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

using Xamarin.Forms;

namespace MooseDrive.ViewModels
{
    public class ServiceViewModel : BindableModel
    {
        public BLEDevice Device { get; private set; }
        public IService Service { get; private set; }
        public string Id { get; set; }
        public string Name { get; set; }

        List<CharacteristicViewModel> _characteristics = new List<CharacteristicViewModel>();
        public List<CharacteristicViewModel> Characteristics
        {
            get => _characteristics;
            set => SetProperty(ref _characteristics, value);
        }

        public ServiceViewModel() { }

        public ServiceViewModel(BLEDevice device, IService service) : this()
        {
            this.Device = device;
            this.Service = service;

            Id = service.Id.ToString();
            Name = service.Name;
        }

        public async Task FetchCharacteristicsAsync()
        {
            try
            {
                Status = TaskStatuses.Busy;
                var characteristics = await Service.GetCharacteristicsAsync();
                if (characteristics != null)
                {
                    Characteristics = characteristics.Select(x => new CharacteristicViewModel(
                        Device, Service, x)).ToList();
                }
            }
            finally
            {
                Status = TaskStatuses.Success;
            }
        }
    }
}
