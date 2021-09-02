using Moose;

using MooseDrive.Mobile.App.Services.Implementations;
using MooseDrive.Models;

using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

using Xamarin.Forms;

[assembly: Dependency(typeof(SessionService))]
namespace MooseDrive.Mobile.App.Services.Implementations
{
    public class SessionService : ISessionService
    {
        readonly IDatabaseService databaseService;
        readonly ILocationService locationService;
        readonly IBluetoothService bluetoothService;
        string sessionId;
        long sequenceId;

        public SessionService()
        {
            databaseService = DependencyService.Get<IDatabaseService>();
            locationService = DependencyService.Get<ILocationService>();
            bluetoothService = DependencyService.Get<IBluetoothService>();
        }

        public void Setup()
        {
            bluetoothService.Connected += BluetoothService_Connected;
            bluetoothService.Disconnected += BluetoothService_Disconnected;
        }

        private void BluetoothService_Connected(object sender, ELMDevice e)
        {
            sessionId = IdExtensions.GenerateId();
            sequenceId = 0;

            Task.Run(databaseService.Db.CompactAsync);

            e.Driver.OnResponseToMessage += Driver_OnResponseToMessage;
            Device.BeginInvokeOnMainThread(async () =>
            {
                await locationService.StartAsync();
            });
        }

        private void BluetoothService_Disconnected(object sender, ELMDevice e)
        {
            Device.BeginInvokeOnMainThread(async () =>
            {
                await locationService.StopAsync();
            });
        }


        private void Driver_OnResponseToMessage(object sender, Messages.ELMMessage e)
        {
            var value = 0;
            if (e.GetResult() is int v) {
                value = v;
            }
            var model = new OBDResponse
            {
                Code = e.Message,
                Response = e.LastInput,
                Value = value,
                Timestamp = DateTimeOffset.Now,
                SessionId = sessionId,
                SequenceId = sequenceId++
            };
            if (sequenceId == long.MaxValue)
            {
                sessionId = IdExtensions.GenerateId();
                sequenceId = 0;
            }
            if (locationService.LastKnown != null)
            {
                model.Latitude = locationService.LastKnown.Latitude;
                model.Longitude = locationService.LastKnown.Longitude;
                model.LocationTimestamp = locationService.LastTimestamp;
            }
            Task.Run(() => databaseService.ELMLogger.AddAsync(model));
        }
    }
}
