using Microsoft.AppCenter;
using Microsoft.AppCenter.Crashes;

using MooseDrive.Services.Implementations;

using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

using Xamarin.Forms;

[assembly: Dependency(typeof(ErrorService))]
namespace MooseDrive.Services.Implementations
{
    public class ErrorService : IErrorService
    {
        public void AlertError(string message)
        {
            App.Instance.DisplayAlert("Error", message, "OK");
        }

        public async Task AlertErrorAsync(string message)
        {
            await App.Instance.DisplayAlertAsync("Error", message, "OK");
        }

        public void Report(Exception ex)
        {
            Crashes.TrackError(ex);
        }

        public void ReportAndThrow(Exception ex)
        {
            Report(ex);
            throw ex;
        }
    }
}
