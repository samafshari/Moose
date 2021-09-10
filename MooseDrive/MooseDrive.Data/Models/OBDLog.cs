using System;
using System.Collections.Generic;
using System.Text;

namespace MooseDrive.Models
{
    public class OBDLog
    {
        public DateTimeOffset Timestamp { get; set; }
        public DateTime LocalTimestamp => Timestamp.LocalDateTime;
        public string Data { get; set; }
        public bool IsOutgoing { get; set; }
        public override string ToString()
        {
            return $"[{LocalTimestamp:HH:mm}] {(IsOutgoing ? "<-" : "->")} {Data}";
        }
    }
}
