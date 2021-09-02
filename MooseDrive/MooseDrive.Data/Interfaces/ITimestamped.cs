using System;
using System.Collections.Generic;
using System.Text;

namespace MooseDrive.Interfaces
{
    public interface ITimestamped
    {
        DateTimeOffset Timestamp { get; set; }
    }
}
