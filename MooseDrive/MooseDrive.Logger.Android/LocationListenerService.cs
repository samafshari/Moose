using Android.App;
using Android.Content;
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

namespace MooseDrive.Logger.Android
{
    public class LocationListenerService : Service
    {
        public static LocationListenerService Instance { get; private set; }

        Messenger messenger;
        public Messenger ReplyToMessenger;
        public bool IsForeground { get; private set; }
        public string NotificationTitle { get; } = "Location Services";
        public string NotificationText { get; } = "Monitoring location changes";
        public string NotificationChannelId { get; }
        public int NotificationSmallIcon { get; }
        public Type ActivityType { get; }

        public event EventHandler<Position> OnPositionChange;
        public event EventHandler<bool> OnStatusChange;

        public void Log(string s, [CallerMemberName] string m = null)
            => Console.WriteLine($"[SensorService] [{m}] {s}");

        public LocationListenerService()
        {
            Instance = this;
        }

        public override IBinder OnBind(Intent intent)
        {
            CreateMessenger();
            return messenger.Binder;
        }

        void CreateMessenger()
        {
            if (messenger == null)
                messenger = new Messenger(default(Handler));
        }

        public override StartCommandResult OnStartCommand(Intent intent, [GeneratedEnum] StartCommandFlags flags, int startId)
        {
            return StartCommandResult.Sticky;
        }

        public void SetForegroundState(bool flag, string text = "")
        {
            if (string.IsNullOrWhiteSpace(text))
                text = NotificationText;

            if (flag)
            {
                if (IsForeground) return;
                IsForeground = true;

                var notification = new NotificationCompat.Builder(this)
                    .Customize(ActivityType, this)
                    .SetContentTitle(NotificationTitle)
                    .SetContentText(text)
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

        LocationListener locationListener = null;
        public void StartLocationListener()
        {
            if (locationListener == null)
            {
                locationListener = new LocationListener(this);
                locationListener.OnLocation += LocationListener_OnLocation;
            }

            locationListener.StartLocationListener();
            OnStatusChange?.Invoke(this, true);
        }

        public void StopLocationListener()
        {
            if (locationListener == null)
                locationListener = new LocationListener(this);

            locationListener.OnLocation -= LocationListener_OnLocation;
            locationListener.StopLocationListener();
            OnStatusChange?.Invoke(this, false);
        }

        public override void OnCreate()
        {
            base.OnCreate();
            CreateMessenger();
            StartLocationListener();
        }

        public override void OnDestroy()
        {
            StopLocationListener();
            base.OnDestroy();
        }

        void LocationListener_OnLocation(object sender, PositionEventArgs e)
        {
            OnPositionChange?.Invoke(this, e.Position);
        }
    }
}