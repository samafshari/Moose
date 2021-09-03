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
        long obdSeqId;
        long locationSeqId;

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

            locationService.OnLocation += LocationService_OnLocation;
        }

        private void BluetoothService_Connected(object sender, ELMDevice e)
        {
            sessionId = IdExtensions.GenerateId();
            obdSeqId = 0;
            locationSeqId = 0;
            databaseService.Use(sessionId);

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
                SequenceId = obdSeqId++
            };
            if (obdSeqId == long.MaxValue || locationSeqId == long.MaxValue)
            {
                sessionId = IdExtensions.GenerateId();
                obdSeqId = 0;
                locationSeqId = 0;
            }
            if (locationService.LastKnown != null)
            {
                model.Latitude = locationService.LastKnown.Latitude;
                model.Longitude = locationService.LastKnown.Longitude;
                model.LocationTimestamp = locationService.LastTimestamp;
            }
            Task.Run(() => databaseService.ELMLogger.AddAsync(model));
        }

        private void LocationService_OnLocation(object sender, Xamarin.Essentials.Location e)
        {
            if (e == null) return;
            var model = new LocationReading
            {
                Accuracy = e.Accuracy ?? -1,
                Latitude = e.Latitude,
                Longitude = e.Longitude,
                SequenceId = locationSeqId++,
                SessionId = sessionId,
                Speed = e.Speed ?? -1,
                Timestamp = DateTimeOffset.Now
            };
            Task.Run(() => databaseService.LocationLogger.AddAsync(model));
        }

    }
}
