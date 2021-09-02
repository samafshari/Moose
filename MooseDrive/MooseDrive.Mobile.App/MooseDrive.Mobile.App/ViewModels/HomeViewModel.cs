using MooseDrive.Mobile.App.Services;

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
    public class HomeViewModel : BindableModel
    {
        ELMDevice device;

        public IBluetoothService BluetoothService { get; }
        public ISettingsService SettingsService { get; }

        public bool IsConnected { get; set; } = false;
        public string DeviceName => device?.Name;

        public int RPM { get; private set; }
        public int Speed { get; private set; }
        public int MAF { get; private set; }
        public int EngineLoad { get; private set; }

        public enum Tabs
        {
            Home,
        }

        int _selectedIndex;
        public int SelectedIndex
        {
            get => _selectedIndex;
            set
            {
                SetProperty(ref _selectedIndex, value);
                RaisePropertyChanged(nameof(Tab));
            }
        }

        public Tabs Tab
        {
            get => (Tabs)SelectedIndex;
            set => SelectedIndex = (int)value;
        }

        public HomeViewModel()
        {
            RaisePropertyChangeOnUI = true;

            BluetoothService = DependencyService.Get<IBluetoothService>();
            SettingsService = DependencyService.Get<ISettingsService>();

            Status = TaskStatuses.Success;

            BluetoothService.Connected += Agent_SupportedDeviceConnected;
            BluetoothService.Disconnected += Agent_SupportedDeviceDisconnected;
            BluetoothService.OnUpdate += (s, e) => Refresh();
        }

        public override void OnStart()
        {
            DeviceDisplay.KeepScreenOn = true;
            base.OnStart();
        }

        private void Agent_SupportedDeviceConnected(object sender, ELMDevice result)
        {
            IsConnected = true;
            device = result;

            UpdateProperties();
        }
        private void Agent_SupportedDeviceDisconnected(object sender, ELMDevice result)
        {
            if (device == null) return;
            IsConnected = false;
            device = null;
            UpdateProperties();
        }

        public Command ConnectCommand => new Command(async () =>
        {
            if (IsConnected) DisconnectCommand.Execute(null);
            else await App.Instance.ShowModalPageAsync(new Views.ConnectPage());
        });

        public Command DisconnectCommand => new Command(async () =>
        {
            if (IsConnected)
            {
                var ask = await App.Instance.DisplayAlertAsync(
                    "Disconnect",
                    $"Would you like to disconnect from {DeviceName}?",
                    "Disconnect", "Cancel");
                if (ask) await BluetoothService.DisconnectAsync();
            }
        });

        public override void Refresh()
        {
            base.Refresh();
            var driver = device?.Driver;
            if (driver != null)
            {
                RPM = driver.RPM;
                Speed = driver.Speed;
                MAF = driver.MAF;
                EngineLoad = driver.EngineLoad;
            }

            UpdateProperties();
        }
    }
}
