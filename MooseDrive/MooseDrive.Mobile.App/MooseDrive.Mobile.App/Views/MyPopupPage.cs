using Rg.Plugins.Popup.Animations;
using Rg.Plugins.Popup.Enums;
using Rg.Plugins.Popup.Pages;

using System;
using System.Collections.Generic;
using System.Text;

namespace MooseDrive.Mobile.App.Views
{
    public class MyPopupPage : PopupPage
    {
        public MyPopupPage()
        {
            Animation = new ScaleAnimation
            {
                PositionIn = MoveAnimationOptions.Center,
                PositionOut = MoveAnimationOptions.Center,
                ScaleIn = 1.2,
                ScaleOut = 0.8,
                DurationIn = 400,
                DurationOut = 300,
                EasingIn = Xamarin.Forms.Easing.SinOut,
                EasingOut = Xamarin.Forms.Easing.SinIn,
                HasBackgroundAnimation = false
            };
        }
    }
}
