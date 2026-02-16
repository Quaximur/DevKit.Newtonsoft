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

        public Migrator(params IMigration[] migrations)
        {
            _migrations = migrations == null ? 
                Array.Empty<IMigration>() : 
                migrations.OrderBy(x => x.ToVersion).ToArray();
        }
        
        public Migrator(IEnumerable<IMigration> migrations)
        {
            _migrations = migrations == null ? 
                Array.Empty<IMigration>() : 
                migrations.OrderBy(x => x.ToVersion).ToArray();
        }

        public bool TryMigrateIfNecessary<T>(string loadedData, out T upToDateData) where T : SaveStateBase
        {
            var dataObject = JObject.Parse(loadedData);
            
            var versionToken = dataObject["_version"] ?? dataObject["Version"];

            if (versionToken == null)
            {
                FLogger.LogWarning("Could not find version field in loaded data.");
                upToDateData = dataObject.ToObject<T>();
                
                return false;
            }
            
            var version = versionToken.ToObject<int>();
            FLogger.Log<Migrator>($"Current version: {version}");

            if (_migrations.Length == 0 || version >= _migrations.Last().ToVersion)
            {
                upToDateData = JsonConvert.DeserializeObject<T>(loadedData);
                return false;
            }

            foreach (var migration in _migrations)
            {
                if (version < migration.ToVersion)
                {
                    dataObject = migration.Migrate(dataObject);
                    version = migration.ToVersion;
                    dataObject[versionToken.ToString()] = version.ToString();
                }
            }

            upToDateData = dataObject.ToObject<T>();
            
            return true;
        }
    }
}