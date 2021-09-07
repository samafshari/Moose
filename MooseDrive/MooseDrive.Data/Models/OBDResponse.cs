using MooseDrive.Interfaces;

using System;
using System.Collections.Generic;
using System.Text;

namespace MooseDrive.Models
{
    public class OBDResponse : IOBDResponse
    {
        public string Id { get; set; }
        public long SequenceId { get; set; }
        public string SessionId { get; set; }
        public string Code { get; set; }
        public string Response { get; set; }
        public int Value { get; set; }
        public double Latitude { get; set; }
        public double Longitude { get; set; }
        public DateTimeOffset LocationTimestamp { get; set; }
        public DateTimeOffset Timestamp { get; set; }
        public bool IsResponseValid { get; set; }
    }
}
