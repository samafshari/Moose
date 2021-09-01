using Moose.Mobile.BLE;

using Plugin.BLE.Abstractions.Contracts;

using System;
using System.Collections.Generic;
using System.Text;

namespace MooseDrive
{
    public class ELMDevice : SupportedBLEDevice<ELMDriver>
    {
        public ELMDevice(
            IDevice device,
            IService service,
            ICharacteristic readCharacteristic,
            ICharacteristic writeCharacteristic) :
            base(device, service, readCharacteristic, writeCharacteristic)
        {
        }
    }
}
