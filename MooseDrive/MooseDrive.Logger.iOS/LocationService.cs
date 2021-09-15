using CoreLocation;

using Foundation;

using Plugin.Geolocator.Abstractions;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

using UIKit;

namespace MooseDrive.Logger.iOS
{
    internal class LocationService : IGeolocator
    {
        bool deferringUpdates;
        readonly CLLocationManager manager;
        bool isListening;
        Position position;
        ListenerSettings listenerSettings;

        public LocationService()
        {
            DesiredAccuracy = 100;
            manager = GetManager();
            manager.AuthorizationChanged += OnAuthorizationChanged;
            manager.Failed += OnFailed;

            if (UIDevice.CurrentDevice.CheckSystemVersion(6, 0))
                manager.LocationsUpdated += OnLocationsUpdated;
            else
                manager.UpdatedLocation += OnUpdatedLocation;

            manager.UpdatedHeading += OnUpdatedHeading;
            manager.DeferredUpdatesFinished += OnDeferredUpdatedFinished;

            RequestAuthorization();

            if (IsGeolocationEnabled)
            {
                manager.StartMonitoringSignificantLocationChanges();
            }
        }


        void OnDeferredUpdatedFinished(object sender, NSErrorEventArgs e) => deferringUpdates = false;
        bool CanDeferLocationUpdate => CLLocationManager.DeferredLocationUpdatesAvailable && UIDevice.CurrentDevice.CheckSystemVersion(6, 0);

        public event EventHandler<PositionErrorEventArgs> PositionError;
        public event EventHandler<PositionEventArgs> PositionChanged;

        /// <summary>
        /// Desired accuracy in meters
        /// </summary>
        public double DesiredAccuracy
        {
            get;
            set;
        }

        /// <summary>
        /// Gets if you are listening for location changes
        ///
        public bool IsListening => isListening;

        public bool SupportsHeading => CLLocationManager.HeadingAvailable;
        public bool IsGeolocationAvailable => true; //all iOS devices support Geolocation

        public bool IsGeolocationEnabled
        {
            get
            {
                var status = CLLocationManager.Status;

                if (UIDevice.CurrentDevice.CheckSystemVersion(8, 0))
                {
                    return CLLocationManager.LocationServicesEnabled && (status == CLAuthorizationStatus.AuthorizedAlways
                    || status == CLAuthorizationStatus.AuthorizedWhenInUse);
                }

                return CLLocationManager.LocationServicesEnabled && status == CLAuthorizationStatus.Authorized;
            }
        }

        void RequestAuthorization()
        {
            var info = NSBundle.MainBundle.InfoDictionary;

            if (UIDevice.CurrentDevice.CheckSystemVersion(8, 0))
            {
                manager.RequestAlwaysAuthorization();
                manager.AllowsBackgroundLocationUpdates = true;
                manager.PausesLocationUpdatesAutomatically = false;
            }
        }

        public Task<Position> GetLastKnownLocationAsync()
        {
            var m = GetManager();
            var newLocation = m?.Location;

            if (newLocation == null)
                return null;

            var position = new Position();
            position.Accuracy = newLocation.HorizontalAccuracy;
            position.Altitude = newLocation.Altitude;
            position.AltitudeAccuracy = newLocation.VerticalAccuracy;
            position.Latitude = newLocation.Coordinate.Latitude;
            position.Longitude = newLocation.Coordinate.Longitude;
            position.Bearing = newLocation.Course;
            position.Speed = newLocation.Speed;

            try
            {
                position.Timestamp = new DateTimeOffset(newLocation.Timestamp.ToDateTime());
            }
            catch (Exception)
            {
                position.Timestamp = DateTimeOffset.UtcNow;
            }

            return Task.FromResult(position);
        }

        public Task<Position> GetPositionAsync(TimeSpan? timeout, CancellationToken? cancelToken = null, bool includeHeading = true)
        {
            throw new NotImplementedException("GetPositionAsync is not implemented.");
        }

        public Task<bool> StartListeningAsync(TimeSpan minTime, double minDistance, bool includeHeading = true, ListenerSettings settings = null, object context = null)
        {
            if (minDistance < 0)
                throw new ArgumentOutOfRangeException(nameof(minDistance));
            if (isListening)
                throw new InvalidOperationException("Already listening");

            if (settings == null) settings = new ListenerSettings();
            listenerSettings = settings;

            var desiredAccuracy = DesiredAccuracy;

            if (UIDevice.CurrentDevice.CheckSystemVersion(6, 0))
            {
                manager.PausesLocationUpdatesAutomatically = false; // settings.PauseLocationUpdatesAutomatically;

                switch (settings.ActivityType)
                {
                    case ActivityType.AutomotiveNavigation:
                        manager.ActivityType = CLActivityType.AutomotiveNavigation;
                        break;
                    case ActivityType.Fitness:
                        manager.ActivityType = CLActivityType.Fitness;
                        break;
                    case ActivityType.OtherNavigation:
                        manager.ActivityType = CLActivityType.OtherNavigation;
                        break;
                    default:
                        manager.ActivityType = CLActivityType.Other;
                        break;
                }
            }

            if (!settings.DeferLocationUpdates)
            {
                minDistance = CLLocationDistance.FilterNone;
                desiredAccuracy = CLLocation.AccuracyBest;
            }
            else
            {
                desiredAccuracy = CLLocation.AccuracyThreeKilometers;
            }

            isListening = true;
            manager.ActivityType = CLActivityType.AutomotiveNavigation;
            manager.DesiredAccuracy = desiredAccuracy;
            manager.DistanceFilter = minDistance;

            //if (settings.ListenForSignificantChanges)
            manager.StartMonitoringSignificantLocationChanges();
            //else
            manager.StartUpdatingLocation();

            // if (includeHeading && CLLocationManager.HeadingAvailable)
            manager.StartUpdatingHeading();

            return Task.FromResult(true);
        }

        public Task<bool> StopListeningAsync()
        {
            if (!isListening)
                return Task.FromResult(true);

            isListening = false;
            if (CLLocationManager.HeadingAvailable)
                manager.StopUpdatingHeading();

            // it looks like deferred location updates can apply to the standard service or significant change service. disallow deferral in either case.
            if ((listenerSettings?.DeferLocationUpdates ?? false) && CanDeferLocationUpdate)
                manager.DisallowDeferredLocationUpdates();

            //if (listenerSettings?.ListenForSignificantChanges ?? false)
            manager.StopMonitoringSignificantLocationChanges();
            //else
            manager.StopUpdatingLocation();


            listenerSettings = null;
            position = null;

            return Task.FromResult(true);
        }

        CLLocationManager GetManager()
        {
            CLLocationManager m = null;
            new NSObject().InvokeOnMainThread(() => m = new CLLocationManager());
            return m;
        }

        void OnUpdatedHeading(object sender, CLHeadingUpdatedEventArgs e)
        {
            if (e.NewHeading.TrueHeading == -1)
                return;

            if (position == null)
            {
                return;
            }
            position.Heading = e.NewHeading.TrueHeading;
            position.Bearing = e.NewHeading.TrueHeading;

            //OnPositionChanged(new PositionEventArgs(position));
        }

        void OnLocationsUpdated(object sender, CLLocationsUpdatedEventArgs e)
        {
            foreach (CLLocation location in e.Locations)
                UpdatePosition(location);

            // defer future location updates if requested
            if ((listenerSettings?.DeferLocationUpdates ?? false) && !deferringUpdates && CanDeferLocationUpdate)
            {
                manager.AllowDeferredLocationUpdatesUntil(listenerSettings.DeferralDistanceMeters == null ? CLLocationDistance.MaxDistance : listenerSettings.DeferralDistanceMeters.GetValueOrDefault(),
                    listenerSettings.DeferralTime == null ? CLLocationManager.MaxTimeInterval : listenerSettings.DeferralTime.GetValueOrDefault().TotalSeconds);

                deferringUpdates = true;
            }
        }

        void OnUpdatedLocation(object sender, CLLocationUpdatedEventArgs e) => UpdatePosition(e.NewLocation);

        void UpdatePosition(CLLocation location)
        {
            var p = (position == null) ? new Position() : new Position(this.position);

            if (location.HorizontalAccuracy > -1)
            {
                p.Accuracy = location.HorizontalAccuracy;
                p.Latitude = location.Coordinate.Latitude;
                p.Longitude = location.Coordinate.Longitude;
            }

            if (location.VerticalAccuracy > -1)
            {
                p.Altitude = location.Altitude;
                p.AltitudeAccuracy = location.VerticalAccuracy;
            }

            if (location.Speed > -1)
                p.Speed = location.Speed;

            try
            {
                var date = location.Timestamp.ToDateTime();
                p.Timestamp = new DateTimeOffset(date);
            }
            catch (Exception)
            {
                p.Timestamp = DateTimeOffset.UtcNow;
            }


            position = p;

            OnPositionChanged(new PositionEventArgs(p));

            location.Dispose();
        }

        void OnPositionChanged(PositionEventArgs e) => PositionChanged?.Invoke(this, e);

        async void OnPositionError(PositionErrorEventArgs e)
        {
            await StopListeningAsync();
            PositionError?.Invoke(this, e);
        }

        void OnFailed(object sender, NSErrorEventArgs e)
        {
            if ((CLError)(int)e.Error.Code == CLError.Network)
                OnPositionError(new PositionErrorEventArgs(GeolocationError.PositionUnavailable));
        }

        void OnAuthorizationChanged(object sender, CLAuthorizationChangedEventArgs e)
        {
            if (e.Status == CLAuthorizationStatus.Denied || e.Status == CLAuthorizationStatus.Restricted)
                OnPositionError(new PositionErrorEventArgs(GeolocationError.Unauthorized));
        }

        public async Task<IEnumerable<Address>> GetAddressesForPositionAsync(Position location)
        {
            if (location == null)
                return null;

            var geocoder = new CLGeocoder();
            var addresses = await geocoder.ReverseGeocodeLocationAsync(new CLLocation(location.Latitude, location.Longitude));

            return addresses.Select(address => new Address
            {
                Longitude = address.Location.Coordinate.Longitude,
                Latitude = address.Location.Coordinate.Latitude,
                FeatureName = address.Name,
                PostalCode = address.PostalCode,
                SubLocality = address.SubLocality,
                CountryCode = address.IsoCountryCode,
                CountryName = address.Country,
                Thoroughfare = address.Thoroughfare,
                SubThoroughfare = address.SubThoroughfare,
                Locality = address.Locality
            });
        }
    }
}