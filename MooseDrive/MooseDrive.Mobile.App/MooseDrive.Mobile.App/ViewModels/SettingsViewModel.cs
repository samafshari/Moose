using Moose.Mobile.Services;

using MooseDrive.Mobile.App.Services;

using RedCorners;
using RedCorners.Forms;
using RedCorners.Models;

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

using Xamarin.Essentials;
using Xamarin.Forms;

namespace MooseDrive.Mobile.App.ViewModels
{
    public class SettingsViewModel : BindableModel
    {
        readonly IDatabaseService databaseService;
        readonly ISettingsService settingsService;

        public SettingsViewModel()
        {
            databaseService = DependencyService.Get<IDatabaseService>();
            settingsService = DependencyService.Get<ISettingsService>();

            Status = TaskStatuses.Success;
            Sessions = databaseService.ListAll().Select(x => new SessionViewModel(x)).ToList();
        }

        public bool AutoConnect
        {
            get => settingsService.Settings.AutoConnect;
            set
            {
                if (settingsService.Settings.AutoConnect != value)
                    settingsService.Settings.AutoConnect = value;
                RaisePropertyChanged();
            }
        }

        public List<SessionViewModel> Sessions { get; }

        public string LastDeviceName => settingsService.Settings.LastDeviceName;

        public Command ExportDatabaseCommand => new Command(async () =>
        {
            var path = databaseService.Db.Path;
            await Share.RequestAsync(new ShareFileRequest
            {
                File = new ShareFile(path),
                Title = "Export Database"
            });
        });

        public Command ImportDatabaseCommand => new Command(async () =>
        {
            var customFileType =
                new FilePickerFileType(new Dictionary<DevicePlatform, IEnumerable<string>>
                {
                    { DevicePlatform.iOS, new[] { "public.data" } },
                    { DevicePlatform.Android, new[] { Vars.DatabaseExtension } },
                    { DevicePlatform.UWP, new[] { Vars.DatabaseExtension } },
                    { DevicePlatform.Tizen, new[] {Vars.DatabaseExtension } },
                    { DevicePlatform.macOS, new[] { Vars.DatabaseExtension } }
                });

            var file = await FilePicker.PickAsync(new PickOptions
            {
                PickerTitle = "Import Database",
                FileTypes = customFileType
            });

            if (string.IsNullOrWhiteSpace(file?.FullPath))
                return;

            if (!File.Exists(file.FullPath))
            {
                App.Instance.DisplayAlert("Error", $"File {file.FullPath} not found or cannot be opened.", "OK");
                return;
            }

            var ask = await App.Instance.DisplayAlertAsync("Replace Current Database",
                "By continuing this process, your existing data will be deleted forever. Do you want to continue?",
                "Delete Current Data and Import",
                "Cancel");

            if (!ask) return;

            ask = await App.Instance.DisplayAlertAsync("Point of No Return",
                "Import and delete current database?", "Continue", "Abort");

            if (!ask) return;

            if (File.Exists(Vars.TempDatabasePath))
                File.Delete(Vars.TempDatabasePath);
            File.Copy(file.FullPath, Vars.TempDatabasePath);
            await databaseService.Db.ImportAsync(Vars.TempDatabasePath);

            await App.Instance.PopAllPopupAsync();
            await App.Instance.ShowFirstPageAsync();
            await App.Instance.DisplayAlertAsync(
                "Import Successful",
                "The new database has been imported.",
                "OK");
        });

        public Command DismissCommand => new Command(async () =>
        {
            await App.Instance.PopAllPopupAsync();
            settingsService.Save();
        });
    }
}
