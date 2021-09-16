using MooseDrive.Interfaces;
using MooseDrive.Logger.iOS;

using Plugin.Geolocator.Abstractions;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Xamarin.Forms;

[assembly: Dependency(typeof(LocationListener))]
namespace MooseDrive.Logger.iOS
{
    public class LocationListener : ILocationListener
    {
        public static TimeSpan LocationMinTime => TimeSpan.FromSeconds(30); //ms
        public const double LocationMinDistance = 10; 
        public const double MaxAcceptableAccuracy = 30;
        public const double MinAcceptableSpeed = 1;
        public static ListenerSettings ListenerSettings => new ListenerSettings
        {
            AllowBackgroundUpdates = true,
            PauseLocationUpdatesAutomatically = false,
            DeferralDistanceMeters = LocationMinDistance,
            DeferLocationUpdates = false
        };

        LocationService locationService;

        public bool IsListening => locationService != null;

        public event EventHandler<Position> OnPositionChange;
        public event EventHandler<bool> OnStatusChange;

        public LocationListener()
        {
        }


        public async Task StartAsync()
        {
            if (IsListening) return;
            locationService = new LocationService();
            locationService.PositionChanged += LocationService_PositionChanged;
            await locationService.StartListeningAsync(
                LocationMinTime,
                LocationMinDistance,
                false,
                ListenerSettings);
            OnStatusChange?.Invoke(this, true);
        }
        
        public async Task StopAsync()
        {
            if (!IsListening) return;
            await locationService.StopListeningAsync();
            locationService.PositionChanged -= LocationService_PositionChanged;
            locationService = null;
            OnStatusChange?.Invoke(this, false);
        }

        void LocationService_PositionChanged(object sender, PositionEventArgs e)
        {
            OnPositionChange?.Invoke(this, e.Position);
        }
    }
}
