using RedCorners.Forms;
using RedCorners.Forms.Systems;

using System;

using Xamarin.Forms;
using Xamarin.Forms.Xaml;
using Rg.Plugins.Popup.Pages;
using Rg.Plugins.Popup.Extensions;
using System.Threading.Tasks;
using System.IO;

namespace MooseDrive.Mobile.App
{
    public partial class App : Application2
    {
        public static new App Instance => (App)Application2.Instance;
        public override Page GetFirstPage() => new Views.HomePage();

        public override void InitializeSystems()
        {
            Console.WriteLine("1111111111111111");
            InitializeComponent();
            Console.WriteLine("2222222222222");
            base.InitializeSystems();

            Console.WriteLine("333333333333333");
            UserAppTheme = OSAppTheme.Light;
            Console.WriteLine("44444444444444");
            if (Device.RuntimePlatform == Device.Android)
                NotchSystem.Instance.OverridePadding = new Thickness(0, 0, 0, 0);
            Console.WriteLine("55555555");
        }

        public async Task ShowPopupAsync(PopupPage popup)
        {
            await MainPage.Navigation.PushPopupAsync(popup);
        }

        public async Task PopAllPopupAsync()
        {
            await MainPage.Navigation.PopAllPopupAsync();
        }
    }
}
