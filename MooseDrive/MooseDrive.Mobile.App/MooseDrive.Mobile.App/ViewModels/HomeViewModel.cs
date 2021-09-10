using MooseDrive.Mobile.App.Services;

using RedCorners;
using RedCorners.Forms;
using RedCorners.Models;

using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Xamarin.Essentials;
using Xamarin.Forms;

namespace MooseDrive.Mobile.App.ViewModels
{
    public class HomeViewModel : BindableModel
    {
        public static HomeViewModel Instance { get; private set; }
        public ELMDevice device;

        public IBluetoothService BluetoothService { get; }
        public ISettingsService SettingsService { get; }
        public IDatabaseService DatabaseService { get; }
        public ISessionService SessionService { get; }

        public bool IsConnected { get; set; } = false;
        public string DeviceName => device?.Name;
        public bool IsPaused => device?.Driver?.IsPaused ?? false;
        public int RPM { get; private set; }
        public int Speed { get; private set; }
        public int MAF { get; private set; }
        public int EngineLoad { get; private set; }

        public string Last { get; private set; }

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
            Instance = this;
            RaisePropertyChangeOnUI = true;

            BluetoothService = DependencyService.Get<IBluetoothService>();
            SettingsService = DependencyService.Get<ISettingsService>();
            DatabaseService = DependencyService.Get<IDatabaseService>();
            SessionService = DependencyService.Get<ISessionService>();

            Status = TaskStatuses.Success;

            BluetoothService.Connected += Agent_SupportedDeviceConnected;
            BluetoothService.Disconnected += Agent_SupportedDeviceDisconnected;
            BluetoothService.OnUpdate += (s, e) => Refresh();

            DependencyService.Get<ISessionService>().Setup();
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
            Task.Run(LastAsync);
            UpdateProperties();
        }

        async Task LastAsync()
        {
            if (BluetoothService?.Device?.Driver?.RecentMessages == null)
                return;

            var count = await DatabaseService.ELMLogger.CountAsync();
            var last = await DatabaseService.ELMLogger.GetLastAsync();

            Last = $"Count: {count}\n" +
                $"{DicToStr(BluetoothService.Device.Driver.RecentMessages)}\n" +
                $"Last: {last.SequenceId}, {last.Code}, {last.Response}, {last.Timestamp}\n" +
                $"{last.Latitude}, {last.Longitude}, {last.LocationTimestamp}\n" +
                $"{last.Timestamp - last.LocationTimestamp}";
            RaisePropertyChanged(nameof(Last));
        }

        string DicToStr(ConcurrentDictionary<string, string> s)
        {
            StringBuilder sb = new StringBuilder();
            foreach (var item in s)
            {
                sb.Append(item.Key);
                sb.Append(": ");
                sb.AppendLine(item.Value);
            }
            return sb.ToString();
        }

        public Command SettingsCommand => new Command(async () =>
        {
            await App.Instance.ShowPopupAsync(new Views.SettingsPopup());
        });

        public Command ExportCommand => new Command(async () =>
        {
            await App.Instance.ShowPopupAsync(new Views.ExportDatabasePopup());
        });

        public Command NewSessionCommand => new Command(async () =>
        {
            var ask = await App.Instance.DisplayAlertAsync("New Session", "Start a new session?", "Yes", "No");
            if (ask)
            {
                SessionService.NewSession();
            }
        });

        public Command TerminalCommand => new Command(async () =>
        {
            await App.Instance.ShowModalPageAsync(new Views.TerminalPage());
        });

        public Command PauseCommand => new Command(() =>
        {
            device.Driver.IsPaused = true;
            UpdateProperties();
        });

        public Command ResumeCommand => new Command(() =>
        {
            device.Driver.IsPaused = false;
            UpdateProperties();
        });
    }
}
