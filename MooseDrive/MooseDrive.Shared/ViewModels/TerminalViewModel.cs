using Moose.ELM;

using MooseDrive.Models;

using Plugin.BLE.Abstractions.Contracts;

using RedCorners;
using RedCorners.Forms;
using RedCorners.Models;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Xamarin.Forms;

namespace MooseDrive.ViewModels
{
    public class TerminalViewModel : BindableModel
    {
        int i = 0;

        [NoUpdate]
        public ELMTerminal Terminal { get; private set; }

        [NoUpdate]
        public DeviceViewModel Device { get; private set; }

        [NoUpdate]
        public string Title { get; private set; }

        public TerminalViewModel() { }

        public TerminalViewModel(ELMTerminal terminal)
        {
            this.Terminal = terminal;
            Terminal.OnMessage += Terminal_OnMessage;
            Device = new DeviceViewModel(new BLEDevice(terminal.Device));
            Title = Device.Name;
            Status = TaskStatuses.Success;
        }

        string _command;
        public string Command
        {
            get => _command;
            set => SetProperty(ref _command, value);
        }

        string _log;
        public new string Log
        {
            get => _log;
            set => App.Instance.RunOnUI(() => SetProperty(ref _log, value));
        }

        void AddLog(string message)
        {
            Log = message + "\n" + Log;
        }

        public override void OnStop()
        {
            Terminal.OnMessage -= Terminal_OnMessage;
            base.OnStop();
        }

        public Command SendCommand => new Command(async () =>
        {
            if (string.IsNullOrWhiteSpace(Command)) return;
            AddLog($"[{i++}]W> {Command}");
            var response = await Terminal.WriteAsync($"{Command}\r");
            if (!response) AddLog($"[{i++}]WRITE ERROR");
            Command = null;
        });

        private void Terminal_OnMessage(object sender, string e)
        {
            if (string.IsNullOrWhiteSpace(e))
            {
                i++;
                return;
            }

            if (sender is ICharacteristic characteristic)
            {
                AddLog($"[{i++}]R{characteristic.Uuid}> {e}\n");
            }
        }
    }
}
