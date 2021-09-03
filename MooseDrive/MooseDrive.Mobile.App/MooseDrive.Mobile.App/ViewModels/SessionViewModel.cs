using RedCorners;
using RedCorners.Forms;
using RedCorners.Models;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Xamarin.Essentials;
using Xamarin.Forms;

namespace MooseDrive.Mobile.App.ViewModels
{
    public class SessionViewModel : BindableModel
    {
        public string Path { get; set; }
        public string Title { get; set; }
        public SessionViewModel(string path)
        {
            this.Path = path;
            this.Title = System.IO.Path.GetFileNameWithoutExtension(path);
            Status = TaskStatuses.Success;
        }

        public Command ExportCommand => new Command(async () =>
        {
            await Share.RequestAsync(new ShareFileRequest
            {
                File = new ShareFile(Path),
                Title = "Export Database"
            });
        });
    }
}
