using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;

using MooseDrive.Interfaces;
using MooseDrive.Logger.Android;

using Plugin.Geolocator.Abstractions;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Xamarin.Forms;

[assembly: Dependency(typeof(LocationListener))]
namespace MooseDrive.Logger.Android
{
    public class LocationListener : ILocationListener
    {
        public bool IsListening => LocationListenerService.Instance != null;

        public event EventHandler<Position> OnPositionChange;
        public event EventHandler<bool> OnStatusChange;

        public static string ChannelId = "MooseDrive";
        public static string Title = "MooseDrive";
        public static string Text = "Location monitoring is active";
        public static int SmallIcon = 0;

        public Task StartAsync()
        {
            return StartAsync(Title, Text, ChannelId, SmallIcon);
        }

        public Task StartAsync(string title, string text, string channelId, int smallIcon)
        {
            if (!IsListening)
            {
                Xamarin.Essentials.MainThread.BeginInvokeOnMainThread(() =>
                {
                    LocationListenerService.StartService(title, text, channelId, smallIcon);
                });
            }
            return Task.CompletedTask;
        }

        public Task StopAsync()
        {
            if (IsListening)
            {
                Xamarin.Essentials.MainThread.BeginInvokeOnMainThread(() =>
                {
                    LocationListenerService.Instance.StopSelf();
                });
            }
            return Task.CompletedTask;
        }
    }
}