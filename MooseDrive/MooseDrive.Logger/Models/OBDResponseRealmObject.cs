using MooseDrive.Interfaces;

using Realms;

using System;
using System.Collections.Generic;
using System.Text;

namespace MooseDrive.Logger.Models
{
    public class OBDResponseRealmObject : RealmObject, IOBDResponse
    {
        [Indexed] public string Id { get; set; }
        public long SequenceId { get; set; }
        [Indexed] public string SessionId { get; set; }
        public string Code { get; set; }
        public string Response { get; set; }
        public int Value { get; set; }
        public double Latitude { get; set; }
        public double Longitude { get; set; }
        public DateTimeOffset LocationTimestamp { get; set; }
        [Indexed] public DateTimeOffset Timestamp { get; set; }
    }
}
