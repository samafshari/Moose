using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

using Xamarin.Forms;

namespace MooseDrive.Mobile.App
{
    public static class Vars
    {
        public static bool IsIOS => Device.RuntimePlatform == Device.iOS;
        public static Color ShadowColor => IsIOS ? Color.FromHex("#33000000") : Color.FromHex("#AA000000");
        public static float ShadowRadius => IsIOS ? 20.0f : 20.0f;
        public static string FontRegular => null;// IsIOS ? "Nunito-Regular" : "Nunito-Regular.ttf#Nunito-Regular";
        public static string FontSemibold => null;// IsIOS ? "Nunito-SemiBold" : "Nunito-SemiBold.ttf#Nunito-SemiBold";
        public static string DatabaseExtension => "realm";
        public static string StorageDirectory => Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
        public static string DatabasePath => Path.Combine(StorageDirectory, $"moosedrive.{DatabaseExtension}");
        public static string TempDatabasePath => Path.Combine(StorageDirectory, $"moosedrivet.{DatabaseExtension}");
        public static int RealtimeFetchDelayMs => 1000;
    }
}
