using System;
using System.Collections.Generic;
using System.Text;

namespace MooseDrive.Interfaces
{
    public interface IOBDResponse : ITimestamped
    {
        string Id { get; }
        long SequenceId { get; }
        string SessionId { get; }

        string Code { get; }
        string Response { get; }
        int Value { get; }

        double Latitude { get; }
        double Longitude { get; }
        DateTimeOffset LocationTimestamp { get; }
    }
}
