using System;
using System.Collections.Generic;
using System.Text;

namespace Moose.Mobile.BLE
{
    public class BLEDevice
    {
        public string Id { get; private set; }
        public string Name { get; private set; }
        public bool IsSupported { get; protected set; }

        public BLEDevice(string id, string name)
        {
            this.Id = id;
            this.Name = name;
        }
    }
}
