using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using System.Threading;
using Android.Locations;
using Plugin.Geolocator.Abstractions;
using ALocationManager = Android.Locations.LocationManager;

namespace MooseDrive.Logger.Android.Plugins
{
    [Preserve(AllMembers = true)]
    class GeolocationContinuousListener
      : Java.Lang.Object, ILocationListener
    {
        IList<string> providers;
        readonly HashSet<string> activeProviders = new HashSet<string>();
        readonly ALocationManager manager;

        string activeProvider;
        Location lastLocation;
        TimeSpan timePeriod;

        public GeolocationContinuousListener(ALocationManager manager, TimeSpan timePeriod, IList<string> providers)
        {
            this.manager = manager;
            this.timePeriod = timePeriod;
            this.providers = providers;

            foreach (string p in providers)
            {
                if (manager.IsProviderEnabled(p))
                    activeProviders.Add(p);
            }
        }

        public event EventHandler<PositionErrorEventArgs> PositionError;
        public event EventHandler<PositionEventArgs> PositionChanged;

        int mockPenalty = 0;
        public void OnLocationChanged(Location location)
        {
            if (location.Provider != activeProvider)
            {
                if (activeProvider != null && manager.IsProviderEnabled(activeProvider))
                {
                    var pr = manager.GetProvider(location.Provider);
                    var lapsed = GetTimeSpan(location.Time) - GetTimeSpan(lastLocation.Time);
                    /*
                    if (pr.Accuracy > manager.GetProvider(activeProvider).Accuracy
                      && lapsed < timePeriod.Add(timePeriod))
                    {
                        location.Dispose();
                        return;
                    }*/
                }

                activeProvider = location.Provider;
            }

            var previous = Interlocked.Exchange(ref lastLocation, location);
            if (previous != null)
                previous.Dispose();

            PositionChanged?.Invoke(this, new PositionEventArgs(location.ToPosition()));
        }

        public void OnProviderDisabled(string provider)
        {
            if (provider == ALocationManager.PassiveProvider)
                return;

            lock (activeProviders)
            {
                if (activeProviders.Remove(provider) && activeProviders.Count == 0)
                    OnPositionError(new PositionErrorEventArgs(GeolocationError.PositionUnavailable));
            }
        }

        public void OnProviderEnabled(string provider)
        {
            if (provider == ALocationManager.PassiveProvider)
                return;

            lock (activeProviders)
                activeProviders.Add(provider);
        }

        public void OnStatusChanged(string provider, Availability status, Bundle extras)
        {
            switch (status)
            {
                case Availability.Available:
                    OnProviderEnabled(provider);
                    break;

                case Availability.OutOfService:
                    OnProviderDisabled(provider);
                    break;
            }
        }

        private TimeSpan GetTimeSpan(long time) => new TimeSpan(TimeSpan.TicksPerMillisecond * time);


        private void OnPositionError(PositionErrorEventArgs e) => PositionError?.Invoke(this, e);

    }

    public static class PositionExtensions
    {
        public static Position ToPosition(this Location location)
        {
            var p = new Position();
            if (location.HasAccuracy)
                p.Accuracy = location.Accuracy;
            if (location.HasAltitude)
                p.Altitude = location.Altitude;
            if (location.HasBearing)
            {
                p.Heading = location.Bearing;
                p.Bearing = location.Bearing;
            }
            if (location.HasSpeed)
                p.Speed = location.Speed;

            p.Longitude = location.Longitude;
            p.Latitude = location.Latitude;
            p.Timestamp = location.GetTimestamp();
            return p;
        }

        static readonly DateTime Epoch = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);
        static DateTimeOffset GetTimestamp(this Location location)
        {
            try
            {
                return new DateTimeOffset(Epoch.AddMilliseconds(location.Time));
            }
            catch (Exception)
            {
                return new DateTimeOffset(Epoch);
            }
        }
    }
}