using Moose.Mobile.BLE;

using Plugin.BLE.Abstractions.Contracts;

using System;
using System.Collections.Generic;
using System.Text;

namespace MooseDrive
{
    public class ELMAgent : BLEAgent<ELMDevice, ELMDriver>
    {
        public ELMAgent()
        {
            ServiceUuid = "E7810A71-73AE-499D-8C15-FAA9AEF0C3F2";
            ReadCharacteristicUuid = WriteCharacteristicUuid = "BEF8D6C9-9C21-4C9E-B632-BD58C1009F9F";
        }

        protected override ELMDevice CreateDevice(IDevice device, IService service, ICharacteristic readCharacteristic, ICharacteristic writeCharacteristic)
        {
            return new ELMDevice(device, service, readCharacteristic, writeCharacteristic);
        }
    }
}
