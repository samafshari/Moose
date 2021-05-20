using Moose.ELM;

using MooseDrive.Models;

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
            set => SetProperty(ref _log, value);
        }

        void AddLog(string message)
        {
            Log = message + "\n" + Log;
        }

        public Command SendCommand => new Command(async () =>
        {
            if (string.IsNullOrWhiteSpace(Command)) return;
            AddLog($"W> {Command}");
            var response = await Terminal.WriteAsync(Command);
            if (response == null) AddLog("R> NULL");
            else AddLog($"R> {response}");
            Command = null;
        });
    }
}
