using Android.App;
using Android.Content;
using Android.Gms.Common;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;

using AndroidX.Core.App;

using Plugin.Geolocator.Abstractions;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;

[assembly: UsesPermission(global::Android.Manifest.Permission.ForegroundService)]
[assembly: UsesPermission(global::Android.Manifest.Permission.AccessBackgroundLocation)]
[assembly: UsesPermission(Android.Manifest.Permission.AccessFineLocation)]
[assembly: UsesPermission(Android.Manifest.Permission.AccessCoarseLocation)]
namespace MooseDrive.Logger.Android
{
    [Service]
    public class LocationListenerService : Service
    {
        LocationManager locationManager = null;
        public static LocationListenerService Instance { get; private set; }

        public static void StartService(string title, string text, string channelId, int smallIcon)
        {
            var activity = Xamarin.Essentials.Platform.CurrentActivity;
            StartService(activity, title, text, channelId, smallIcon);
        }

        public static void StartService(Activity activity, string title, string text, string channelId, int smallIcon)
        {
            var intent = new Intent(activity, typeof(LocationListenerService));
            intent.PutExtra(nameof(NotificationTitle), title);
            intent.PutExtra(nameof(NotificationText), text);
            intent.PutExtra(nameof(NotificationChannelId), channelId);
            intent.PutExtra(nameof(NotificationSmallIcon), smallIcon);
            activity.StartService(intent);
        }

        public static void CreateNotificationChannel(Activity activity, string channelId, string name, NotificationImportance importance)
        {
            if (!IsPlayServicesAvailable(activity))
                return;

            // No need to create a channel for APIs less than O
            if (Build.VERSION.SdkInt < BuildVersionCodes.O)
                return;

            NotificationManager notificationManager = 
                (NotificationManager)activity.GetSystemService(NotificationService);

            var channel = new NotificationChannel(channelId, name, importance);
            channel.EnableLights(true);
            channel.EnableVibration(false);
            channel.SetSound(null, null);
            channel.LockscreenVisibility = NotificationVisibility.Public;
            notificationManager.CreateNotificationChannel(channel);
        }

        public static bool IsPlayServicesAvailable(Activity activity)
        {
            int resultCode = GoogleApiAvailability.Instance.IsGooglePlayServicesAvailable(activity);
            if (resultCode != ConnectionResult.Success)
            {
                if (GoogleApiAvailability.Instance.IsUserResolvableError(resultCode))
                {
                    Console.WriteLine($"IsPlayServicesAvailable: false, {GoogleApiAvailability.Instance.GetErrorString(resultCode)}");
                    return false;
                }
                else
                {
                    Console.WriteLine($"IsPlayServicesAvailable: FALSE, This device is not supported. Finishing Activity, bye bye");
                    activity.Finish();
                }
                return false;
            }
            else
            {
                Console.WriteLine($"IsPlayServicesAvailable: true, Google Play Services is available.");
                return true;
            }
        }

        public bool IsForeground { get; private set; }
        public string NotificationTitle { get; set; } = "Location Services";
        public string NotificationText { get; set; } = "Monitoring location changes";
        public string NotificationChannelId { get; set; }
        public int NotificationSmallIcon { get; set; }

        public static event EventHandler<Position> OnPositionChange;
        public static event EventHandler<bool> OnStatusChange;

        public LocationListenerService() =>
            Instance = this;

        public override IBinder OnBind(Intent intent)
            => null;

        public override StartCommandResult OnStartCommand(Intent intent, [GeneratedEnum] StartCommandFlags flags, int startId)
        {
            Instance = this;
            if (intent != null)
            {
                if (intent.HasExtra(nameof(NotificationTitle)))
                    NotificationTitle = intent.GetStringExtra(nameof(NotificationTitle)) ?? NotificationTitle;
                if (intent.HasExtra(nameof(NotificationText)))
                    NotificationText = intent.GetStringExtra(nameof(NotificationText)) ?? NotificationText;
                if (intent.HasExtra(nameof(NotificationChannelId)))
                    NotificationChannelId = intent.GetStringExtra(nameof(NotificationChannelId)) ?? NotificationChannelId;
                if (intent.HasExtra(nameof(NotificationSmallIcon)))
                    NotificationSmallIcon = intent.GetIntExtra(nameof(NotificationSmallIcon), NotificationSmallIcon);
            }

            SetForegroundState(true);
            StartLocationListener();

            return StartCommandResult.RedeliverIntent;
        }

        public void Log(string s, [CallerMemberName] string m = null)
            => Console.WriteLine($"[LocationListenerService] [{m}] {s}");

        void SetForegroundState(bool flag)
        {
            if (flag)
            {
                if (IsForeground) return;
                IsForeground = true;

                var notification = new NotificationCompat.Builder(this, NotificationChannelId)
                    .Customize(Xamarin.Essentials.Platform.CurrentActivity, this)
                    .SetContentTitle(NotificationTitle)
                    .SetContentText(NotificationText) 
                    .SetOngoing(true)
                    .SetChannelId(NotificationChannelId)
                    .SetSmallIcon(NotificationSmallIcon)
                    .Build();

                StartForeground(101, notification);
            }
            else
            {
                if (!IsForeground) return;
                IsForeground = false;
                StopForeground(true);
            }
        }

        public void StartLocationListener()
        {
            if (locationManager == null)
            {
                locationManager = new LocationManager(this);
                locationManager.OnLocation += LocationListener_OnLocation;
            }

            locationManager.StartLocationListener();
            OnStatusChange?.Invoke(this, true);
        }

        public void StopLocationListener()
        {
            if (locationManager == null)
                locationManager = new LocationManager(this);

            locationManager.OnLocation -= LocationListener_OnLocation;
            locationManager.StopLocationListener();
            OnStatusChange?.Invoke(this, false);
        }

        public override void OnDestroy()
        {
            SetForegroundState(false);
            StopLocationListener();
            Instance = null;
            base.OnDestroy();
        }

        void LocationListener_OnLocation(object sender, PositionEventArgs e)
        {
            OnPositionChange?.Invoke(this, e.Position);
        }
    }
}