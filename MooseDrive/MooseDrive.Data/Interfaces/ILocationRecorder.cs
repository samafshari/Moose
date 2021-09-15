using System;
using System.Threading.Tasks;
using Plugin.Geolocator.Abstractions;

namespace MooseDrive.Interfaces
{
    public interface ILocationRecorder
    {
        bool IsRecording { get; }
        
        event EventHandler<Position> OnPositionChange;
        event EventHandler<bool> OnStatusChange;

        Task StartAsync();
        Task StopAsync();
    }
}
