using Foundation;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using UIKit;

namespace MooseDrive.Logger.iOS
{
    internal static class DateExtensions
    {
        public static DateTime ToDateTime(this NSDate date)
        {
            return new DateTime(2001, 1, 1, 0, 0, 0).AddSeconds(date.SecondsSinceReferenceDate);
        }

        public static NSDate ToNSDate(this DateTime date)
        {
            return NSDate.FromTimeIntervalSinceReferenceDate((date - new DateTime(2001, 1, 1, 0, 0, 0)).TotalSeconds);
        }

        // Gets the 11:59:59 instance of a DateTime
        public static DateTime AbsoluteEnd(this DateTime dateTime)
        {
            return dateTime.Date.AddDays(1).AddTicks(-1);
        }
    }
}