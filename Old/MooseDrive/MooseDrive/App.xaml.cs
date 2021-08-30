using RedCorners.Forms;

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
        }
    }
}
