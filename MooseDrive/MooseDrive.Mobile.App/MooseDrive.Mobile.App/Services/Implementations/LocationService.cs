using MooseDrive.Mobile.App.Services.Implementations;

using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

using Xamarin.Essentials;
using Xamarin.Forms;

[assembly: Dependency(typeof(LocationService))]
namespace MooseDrive.Mobile.App.Services.Implementations
{
    public class LocationService : ILocationService
    {
        public Location LastKnown { get; private set; }
        public DateTimeOffset LastTimestamp { get; private set; }

        public event EventHandler<Location> OnLocation;

        volatile bool isStarted = false;
        public async Task StartAsync()
        {
            if (isStarted) return;
            isStarted = true;
            while (isStarted)
            {
                DateTime dt = DateTime.Now;
                try
                {
                    var request = new GeolocationRequest(GeolocationAccuracy.Best, TimeSpan.FromSeconds(10));
                    var location = await Geolocation.GetLocationAsync(request);
                    if (location != null)
                    {
                        LastKnown = location;
                        LastTimestamp = DateTimeOffset.Now;
                        OnLocation?.Invoke(this, LastKnown);
                    }
                }
                catch { }
                var opTime = (DateTime.Now - dt).TotalSeconds;
                double delay = 5;
                if (opTime < delay) await Task.Delay(TimeSpan.FromSeconds(delay - opTime));
            }
        }

        public Task StopAsync()
        {
            isStarted = false;
            return Task.CompletedTask;
        }
    }
}
