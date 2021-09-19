using System;
using System.Threading.Tasks;
using Plugin.Geolocator.Abstractions;

namespace MooseDrive.Interfaces
{
    public interface ILocationListener
    {
        bool IsListening { get; }
        
        event EventHandler<Position> OnPositionChange;
        event EventHandler<bool> OnStatusChange;

        Task StartAsync();
        Task StartAsync(string title, string text, string channelId, int smallIcon);
        Task StopAsync();
    }
}
