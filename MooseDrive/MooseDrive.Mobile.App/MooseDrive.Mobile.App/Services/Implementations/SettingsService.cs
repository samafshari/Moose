using Moose.Mobile.Services;
using Moose.Mobile.Services.Implementations;

using MooseDrive.Mobile.App.Models;
using MooseDrive.Mobile.App.Services;
using MooseDrive.Mobile.App.Services.Implementations;

using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

using Xamarin.Forms;

[assembly: Dependency(typeof(SettingsService))]
namespace MooseDrive.Mobile.App.Services.Implementations
{
    public class SettingsService : SettingsServiceBase<Settings>, ISettingsService
    {
    }
}
