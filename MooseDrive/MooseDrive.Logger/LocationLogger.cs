using Moose;

using MooseDrive.Logger.Models;
using MooseDrive.Models;

using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using RedCorners;

namespace MooseDrive.Logger
{
    public class LocationLogger
    {
        readonly Database db;

        public LocationLogger(Database db)
        {
            this.db = db;
        }

        public async Task AddAsync(LocationReading model)
        {
            await Task.Run(() =>
            {
                using (var realm = db.GetRealm())
                    realm.Write(() =>
                    {
                        var record = model.ReturnAs<LocationRealmObject>();
                        record.Id = IdExtensions.GenerateId();
                        realm.Add(record);
                    });
            });
        }
    }
}
