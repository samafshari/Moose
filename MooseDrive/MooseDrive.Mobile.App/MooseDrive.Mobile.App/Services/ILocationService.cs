using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

using Xamarin.Essentials;

namespace MooseDrive.Mobile.App.Services
{
    public interface ILocationService
    {
        event EventHandler<Location> OnLocation;
        
        Location LastKnown { get; }
        DateTimeOffset LastTimestamp { get; }

        Task StartAsync();
        Task StopAsync();
    }
}
