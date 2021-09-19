using MooseDrive.Interfaces;
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
        readonly ILocationListener locationListener;

        public Location LastKnown { get; private set; }
        public DateTimeOffset LastTimestamp { get; private set; }

        public event EventHandler<Location> OnLocation;

        public LocationService()
        {
            locationListener = DependencyService.Get<ILocationListener>();
            locationListener.OnPositionChange += LocationListener_OnPositionChange;
        }

        private void LocationListener_OnPositionChange(object sender, Plugin.Geolocator.Abstractions.Position e)
        {
            OnLocation?.Invoke(this, new Location
            {
                Accuracy = e.Accuracy,
                Altitude = e.Altitude,
                Course = e.Heading,
                Latitude = e.Latitude,
                Longitude = e.Longitude,
                Speed = e.Speed,
                Timestamp = e.Timestamp,
                VerticalAccuracy = e.AltitudeAccuracy
            });
        }

        volatile bool isStarted = false;
        public async Task StartAsync()
        {
            if (isStarted) return;
            isStarted = true;
            await locationListener.StartAsync();
        }

        public async Task StopAsync()
        {
            await locationListener.StopAsync();
            isStarted = false;
        }
    }
}
