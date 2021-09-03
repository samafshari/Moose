using MooseDrive.Interfaces;

using System;
using System.Collections.Generic;
using System.Text;

namespace MooseDrive.Models
{
    public class LocationReading : ILocation
    {
        public string Id { get; set; }
        public long SequenceId { get; set; }
        public string SessionId { get; set; }
        public double Latitude { get; set; }
        public double Longitude { get; set; }
        public double Accuracy { get; set; }
        public double Speed { get; set; }
        public DateTimeOffset Timestamp { get; set; }
    }
}
