using MooseDrive.Interfaces;
using MooseDrive.Logger.iOS;

using Plugin.Geolocator.Abstractions;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Xamarin.Forms;

[assembly: Dependency(typeof(LocationRecorder))]
namespace MooseDrive.Logger.iOS
{
    public class LocationRecorder : ILocationRecorder
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

        public bool IsRecording => locationService != null;

        public event EventHandler<Position> OnPositionChange;
        public event EventHandler<bool> OnStatusChange;

        public LocationRecorder()
        {
        }


        public async Task StartAsync()
        {
            if (IsRecording) return;
            locationService = new LocationService();
            locationService.PositionChanged += LocationService_PositionChanged;
            await locationService.StartListeningAsync(
                LocationMinTime,
                LocationMinDistance,
                false,
                ListenerSettings);
        }
        
        public async Task StopAsync()
        {
            if (!IsRecording) return;
            await locationService.StopListeningAsync();
            locationService.PositionChanged -= LocationService_PositionChanged;
            locationService = null;
        }

        void LocationService_PositionChanged(object sender, PositionEventArgs e)
        {
            OnPositionChange?.Invoke(this, e.Position);
        }
    }
}
