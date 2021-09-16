using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;

using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using Android.Util;
using Plugin.Geolocator;

using ALocationManager = Android.Locations.LocationManager;
using Android.Gms.Common.Apis;
using Android.Gms.Location;
using Android.Support.V4.App;

using Plugin.Geolocator.Abstractions;
using MooseDrive.Logger.Android.Plugins;

namespace MooseDrive.Logger.Android
{
    public class LocationListener
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

        public void Log(string s, [CallerMemberName] string m = null) => Console.WriteLine($"[LocationListener] [{m}] {s}");

        public event EventHandler<PositionEventArgs> OnLocation;

        Context context;

        ALocationManager locationManager;
        GeolocationContinuousListener listener;
        string[] Providers => Manager.GetProviders(enabledOnly: false).ToArray();
        static int instanceCount = 0;

        ALocationManager Manager
        {
            get
            {
                if (locationManager == null)
                    locationManager = (ALocationManager)context.GetSystemService(Context.LocationService);
                return locationManager;
            }
        }

        public LocationListener(Context context)
        {
            this.context = context;
        }

        public bool StartLocationListener()
        {
            if (instanceCount > 0) return false;
            instanceCount++;
            Log($"Location Service Started.");

            var minTimeMilliseconds = LocationMinTime.TotalMilliseconds;

            var providers = Providers;
            listener = new GeolocationContinuousListener(Manager, LocationMinTime, providers);
            listener.PositionChanged += Locator_PositionChanged;

            Looper looper = Looper.MyLooper() ?? Looper.MainLooper;
            for (int i = 0; i < providers.Length; ++i)
            {
                if (providers[i] != ALocationManager.GpsProvider) continue;

                Log($"Found Provider: {providers[i]}");

                Manager.RequestLocationUpdates(providers[i],
                    (long)LocationMinTime.TotalMilliseconds,
                    (float)LocationMinDistance,
                    listener,
                    looper);
            }

            return true;
        }

        public bool StopLocationListener()
        {
            if (instanceCount <= 0) return false;
            try
            {
                instanceCount--;

                var providers = Providers;
                listener.PositionChanged -= Locator_PositionChanged;
                for (int i = 0; i < providers.Length; ++i)
                {
                    if (providers[i] != ALocationManager.GpsProvider) continue;
                    Manager.RemoveUpdates(listener);
                }
            }
            catch (Exception ex)
            {
                Log(ex.ToString());
            }

            return true;
        }

        void Locator_PositionChanged(object sender, PositionEventArgs e)
        {
            Log($"{e.Position.Latitude}, {e.Position.Longitude}");
            OnLocation?.Invoke(sender, e);
        }
    }
}