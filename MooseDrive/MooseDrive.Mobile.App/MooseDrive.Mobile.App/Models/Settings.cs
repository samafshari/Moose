using System;
using System.Collections.Generic;
using System.Text;

namespace MooseDrive.Mobile.App.Models
{
    public class Settings
    {
        public bool AutoConnect { get; set; }
        public string LastDeviceName { get; set; } = "IOS-Vlink";
        public string LastSessionId { get; set; }
        public List<string> CustomMessages { get; set; } = new List<string>();
    }
}
