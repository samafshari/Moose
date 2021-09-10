using MooseDrive.Mobile.App.Services;
using MooseDrive.Models;

using RedCorners.Forms;

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Text;

using Xamarin.Forms;

namespace MooseDrive.Mobile.App.ViewModels
{
    public class TerminalViewModel : BindableModel
    {
        public ObservableCollection<OBDLog> Items { get; set; } = new ObservableCollection<OBDLog>();
        public string Command { get; set; }

        readonly IBluetoothService bluetoothService;

        public TerminalViewModel()
        {
            bluetoothService = DependencyService.Get<IBluetoothService>();
        }

        public override void OnStart()
        {
            base.OnStart();
            HomeViewModel.Instance.device.Driver.OnLog += Driver_OnLog;
        }

        public override void OnStop()
        {
            HomeViewModel.Instance.device.Driver.OnLog -= Driver_OnLog;
            base.OnStop();
        }

        private void Driver_OnLog(object sender, OBDLog e)
        {
            Device.BeginInvokeOnMainThread(() =>
            {
                while (Items.Count > 100)
                    Items.RemoveAt(Items.Count - 1);
                Items.Insert(0, e);
            });
        }

        public Command SendCommand => new Command(() =>
        {
            if (string.IsNullOrWhiteSpace(Command)) return;
            bluetoothService.Device.Driver.InteractiveQueue.Enqueue(Command);
            Command = null;
            RaisePropertyChanged(nameof(Command));
        });
    }
}
