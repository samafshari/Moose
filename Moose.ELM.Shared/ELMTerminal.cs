using Plugin.BLE.Abstractions.Contracts;

using System;
using System.Collections.Generic;
using System.Text;

namespace Moose.ELM
{
    public class ELMTerminal
    {
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
