using Plugin.BLE.Abstractions.Contracts;

using System;
using System.Collections.Generic;
using System.Text;

namespace Moose.ELM
{
    public class ELMTerminal
    {
        public const string ServiceId = "BEF8D6C9-9C21-4C9E-B632-BD58C1009F9F";//"E7810A71-73AE-499D-8C15-FAA9AEF0C3F2";
        public const string CharacteristicUuid = "BEF8D6C9-9C21-4C9E-B632-BD58C1009F9F";

        public IDevice Device { get; private set; }

        public ELMTerminal(IDevice device)
        {
            if (device is null)
                throw new ArgumentNullException(nameof(device), "ELMTerminal constructor needs a non-null IDevice.");

            this.Device = device;
        }

        public void Disconnect()
        {
            
        }
    }
}
