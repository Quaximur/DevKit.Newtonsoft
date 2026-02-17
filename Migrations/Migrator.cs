using System;
using System.Collections.Generic;
using System.Linq;
using DevKit.Saves;
using DevKit.Utils;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace DevKit.Newtonsoft
{
    /// <summary>
    /// For saved data migrating.
    /// Necessary for converting data stored on the device
    /// to the current version in the event of an app update.
    /// </summary>
    public class Migrator
    {
        private readonly IMigration[] _migrations;

        public Migrator(params IMigration[] migrations) : this((IEnumerable<IMigration>)migrations)
        {
        }

        public Migrator(IEnumerable<IMigration> migrations)
        {
            _migrations = migrations == null ?
                Array.Empty<IMigration>() :
                migrations.OrderBy(x => x.ToVersion).ToArray();
        }

        public bool TryMigrateIfNecessary<T>(string loadedData, out T upToDateData) where T : IVersioned
        {
            if (_migrations.Length == 0)
            {
                upToDateData = JsonConvert.DeserializeObject<T>(loadedData);
                return false;
            }

            var dataObject = JObject.Parse(loadedData);
            var versionToken = dataObject["Version"] ?? dataObject["_version"];

            if (versionToken == null)
            {
                FLogger.LogWarning("Could not find 'Version' token in loaded data. No migration will be applied.");
                upToDateData = dataObject.ToObject<T>();

                return false;
            }

            var version = versionToken.ToObject<int>();

            if (version >= _migrations[^1].ToVersion)
            {
                upToDateData = JsonConvert.DeserializeObject<T>(loadedData);
                return false;
            }

            foreach (var migration in _migrations)
            {
                if (version >= migration.ToVersion)
                    continue;

                dataObject = migration.Migrate(dataObject);
                version = migration.ToVersion;
            }

            dataObject[versionToken.ToString()] = version.ToString();
            upToDateData = dataObject.ToObject<T>();

            return true;
        }
    }
}