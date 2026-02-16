using Newtonsoft.Json.Linq;

namespace DevKit.Newtonsoft
{
    public interface IMigration
    {
        public int ToVersion { get; }
        public JObject Migrate(JObject dataObject);
    }
}