using Moose;

using MooseDrive.Models;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

using RedCorners;
using MooseDrive.Logger.Models;

namespace MooseDrive.Logger
{
    public class ELMLogger
    {
        readonly Database db;

        public ELMLogger(Database db)
        {
            this.db = db;
        }

        public async Task AddAsync(OBDResponse model)
        {
            await Task.Run(() =>
            {
                using (var realm = db.GetRealm())
                    realm.Write(() =>
                    {
                        var record = model.ReturnAs<OBDResponseRealmObject>();
                        record.Id = IdExtensions.GenerateId();
                        realm.Add(record);
                    });
            });
        }

        public async Task<OBDResponse> GetLastAsync()
        {
            OBDResponse response = null;
            await Task.Run(() =>
            {
                using (var realm = db.GetRealm())
                {
                    var record = realm.All<OBDResponseRealmObject>()
                        .OrderByDescending(x => x.Timestamp)
                        .FirstOrDefault();
                    response = record.ReturnAs<OBDResponse>();
                }
            });
            return response;
        }

        public async Task<long> CountAsync()
        {
            long result = 0;
            await Task.Run(() =>
            {
                using (var realm = db.GetRealm())
                    result = realm.All<OBDResponseRealmObject>().Count();
            });
            return result;
        }
    }
}
