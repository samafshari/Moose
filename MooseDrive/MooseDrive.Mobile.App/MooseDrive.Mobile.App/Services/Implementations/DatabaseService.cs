using MooseDrive.Mobile.App.Services.Implementations;
using Xamarin.Forms;
using System;
using System.Collections.Generic;
using System.Text;
using Moose;
using MooseDrive.Logger;

[assembly: Dependency(typeof(DatabaseService))]
namespace MooseDrive.Mobile.App.Services.Implementations
{
    public class DatabaseService : IDatabaseService
    {
        public Database Db { get; }
        public ELMLogger ELMLogger { get; }

        public DatabaseService()
        {
            Db = new Database(Vars.DatabasePath);
            ELMLogger = new ELMLogger(Db);
        }
    }
}
