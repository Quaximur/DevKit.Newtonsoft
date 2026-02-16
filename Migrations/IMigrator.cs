using DevKit.Saves;

namespace DevKit.Newtonsoft
{
    public interface IMigrator
    {
        public bool TryMigrateIfNecessary<T>(string loadedData, out T upToDateData) where T : SaveStateBase;
    }
}
