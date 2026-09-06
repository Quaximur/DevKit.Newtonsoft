using System;
using DevKit.Utils;
using Newtonsoft.Json;

namespace DevKit.Saves
{
    public class NewtonsoftSerializer : ISerializer
    {
        public string Serialize<T>(T rawData)
        {
            try
            {
                return JsonConvert.SerializeObject(rawData);
            }
            catch (Exception e)
            {
                FLogger.LogError<NewtonsoftSerializer>(e.Message);
                return null;
            }
        }

        public T Deserialize<T>(string serializedData)
        {
            try
            {
                return JsonConvert.DeserializeObject<T>(serializedData);
            }
            catch (Exception e)
            {
                FLogger.LogError<NewtonsoftSerializer>(e.Message);
                return default;
            }
        }
    }
}
