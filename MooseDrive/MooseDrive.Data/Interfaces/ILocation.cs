using System;
using System.Collections.Generic;
using System.Text;

namespace MooseDrive.Interfaces
{
    public interface ILocation : ITimestamped
    {
        string Id { get; }
        long SequenceId { get; }
        string SessionId { get; }

        double Latitude { get; }
        double Longitude { get; }
        double Accuracy { get; }
        double Speed { get; }
    }
}
