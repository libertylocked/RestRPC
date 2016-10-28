using Newtonsoft.Json;

namespace RestRPC.Framework.Messages
{
    abstract class Message
    {
        [JsonProperty]
        public string Header { get; private set; }

        [JsonProperty]
        public object Data { get; private set; }

        [JsonConstructor]
        public Message(string Header, object Data)
        {
            this.Header = Header;
            this.Data = Data;
        }
    }
}
