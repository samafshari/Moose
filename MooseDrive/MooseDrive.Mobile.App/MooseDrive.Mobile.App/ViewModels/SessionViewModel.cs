using RedCorners;
using RedCorners.Forms;
using RedCorners.Models;

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Xamarin.Essentials;
using Xamarin.Forms;

namespace MooseDrive.Mobile.App.ViewModels
{
    public class SessionViewModel : BindableModel
    {
        public string Path { get; set; }
        public string Title { get; set; }

        public Action UpdateAction;

        public SessionViewModel(string path)
        {
            this.Path = path;
            this.Title = System.IO.Path.GetFileNameWithoutExtension(path);
            var fi = new FileInfo(path);
            Title += $" ({fi.Length / (1024 * 1024)}MB)";
            Status = TaskStatuses.Success;
        }

        async Task ExportAsync()
        {
            await Share.RequestAsync(new ShareFileRequest
            {
                File = new ShareFile(Path),
                Title = "Export Database"
            });
        }

        public Command SelectCommand => new Command(async () =>
        {
            var answers = new[]
            {
                "Export",
                "Delete"
            };
            var choice = await App.Instance.MainPage.DisplayActionSheet(Title, "Cancel", null, answers);
            if (choice == answers[0]) await ExportAsync();
            else if (choice == answers[1])
            {
                var ask = await App.Instance.DisplayAlertAsync("Delete", "Are you sure you want to delete " + Title + "?", "Yes", "No");
                if (ask) await DeleteAsync();
            }
        });

        async Task DeleteAsync()
        {
            File.Delete(Path);
            UpdateAction?.Invoke();
        }
    }
}
