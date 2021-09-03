using MooseDrive.Mobile.App.Services.Implementations;
using Xamarin.Forms;
using System;
using System.Collections.Generic;
using System.Linq;
using Moose;
using MooseDrive.Logger;
using System.IO;

[assembly: Dependency(typeof(DatabaseService))]
namespace MooseDrive.Mobile.App.Services.Implementations
{
    public class DatabaseService : IDatabaseService
    {
        public Database Db { get; private set; }
        public ELMLogger ELMLogger { get; private set; }
        public LocationLogger LocationLogger { get; private set; }

        const ulong SchemaVersion = 1;

        public DatabaseService()
        {
            Directory.CreateDirectory(Vars.DatabaseBasePath);
            Db = new Database(Vars.DefaultDatabasePath, SchemaVersion);
            InitializeDatabase();
        }

        public void Use(string sessionId)
        {
            Db = new Database(Path.Combine(Vars.DatabaseBasePath, $"{sessionId}.{Vars.DatabaseExtension}"), SchemaVersion);
            InitializeDatabase();
        }

        void InitializeDatabase()
        {
            ELMLogger = new ELMLogger(Db);
            LocationLogger = new LocationLogger(Db);
        }

        public List<string> ListAll()
        {
            return Directory.EnumerateFiles(Vars.DatabaseBasePath, $"*.{Vars.DatabaseExtension}").OrderByDescending(x => Path.GetFileName(x)).ToList();
        }
    }
}
