using Realms;

using System;
using System.IO;
using System.Threading.Tasks;

namespace Moose
{
    public class Database
    {
        public virtual ulong SchemaVersion { get; set; }
        public virtual string Path { get; set; }

        public Database()
        {
        }

        public Database(string path)
        {
            Path = path;
        }

        public Database(ulong schemaVersion)
        {
            SchemaVersion = schemaVersion;
        }

        public Database(string path, ulong schemaVersion)
        {
            Path = path;
            SchemaVersion = schemaVersion;
        }

        protected RealmConfiguration GetConfiguration() => GetConfiguration(Path);

        protected virtual RealmConfiguration GetConfiguration(string path)
        {
            return new RealmConfiguration(path)
            {
                SchemaVersion = SchemaVersion,
                MigrationCallback = Migrate
            };
        }

        public virtual Realm GetRealm()
        {
            return Realm.GetInstance(GetConfiguration());
        }

        protected virtual void Migrate(Migration migration, ulong oldSchemaVersion)
        {
        }

        public async Task CompactAsync()
        {
            await Task.Run(() => Realm.Compact(GetConfiguration()));
        }

        public async Task ImportAsync(string path)
        {
            await Task.Run(() =>
            {
                if (!File.Exists(path))
                    throw new ApplicationException($"Cannot import {path}: File does not exist");

                var config = GetConfiguration(path);
                using (var realm = Realm.GetInstance(config))
                {
                    if (realm == null)
                        throw new ApplicationException($"Cannot import {path}: Invalid database");
                    // Good.
                }

                if (File.Exists(Path))
                    File.Delete(Path);
                File.Copy(path, Path);
            });
        }
    }
}
