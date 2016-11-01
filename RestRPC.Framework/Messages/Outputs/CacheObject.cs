using Newtonsoft.Json;

namespace RestRPC.Framework.Messages.Outputs
{
    class CacheObject
    {
        [JsonProperty]
        public string Key { get; private set; }
        
        [JsonProperty]
        public object Value { get; private set; }

        public CacheObject(string Key, object Value)
        {
            this.Key = Key;
            this.Value = Value;
        }
    }
}
