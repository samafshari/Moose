using RedCorners.Forms;
using RedCorners.Forms.Systems;

using System;

using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace MooseDrive
{
    public partial class App : Application2
    {
        public override Page GetFirstPage() => new Views.MainPage();

        public override void InitializeSystems()
        {
            InitializeComponent();
            base.InitializeSystems();

            UserAppTheme = OSAppTheme.Light;

            if (Device.RuntimePlatform == Device.Android)
            {
                NotchSystem.Instance.OverridePadding = new Thickness(0, 0, 0, 0);
            }
        }
    }
}
