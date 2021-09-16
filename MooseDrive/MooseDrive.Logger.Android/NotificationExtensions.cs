using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;

using AndroidX.Core.App;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MooseDrive.Logger.Android
{
    internal static class NotificationExtensions
    {
        public static NotificationCompat.Builder Customize(this NotificationCompat.Builder builder, Type activityType, Context context, IDictionary<string, string> intentParams = null)
        {
            var intent = new Intent(context, activityType);
            intent.AddFlags(ActivityFlags.ClearTop);
            if (intentParams != null)
            {
                foreach (var item in intentParams)
                {
                    intent.PutExtra(item.Key, item.Value);
                }
            }

            var pendingIntent = PendingIntent.GetActivity(context, 0, intent, PendingIntentFlags.CancelCurrent);

            return builder
                .SetDefaults(NotificationCompat.DefaultLights)
                .SetContentIntent(pendingIntent)
                .SetAutoCancel(true);
        }
    }
}