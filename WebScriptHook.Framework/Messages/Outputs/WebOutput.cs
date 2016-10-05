using Newtonsoft.Json;

namespace WebScriptHook.Framework.Messages.Outputs
{
    /// <summary>
    /// This is the format for messages client is sending to the server
    /// </summary>
    abstract class WebOutput
    {
        [JsonProperty]
        public char Header { get; set; }
        [JsonProperty]
        public object[] Data { get; set; }
        [JsonProperty]
        public string UID { get; private set; }

        [JsonConstructor]
        public WebOutput(char Header, object[] Data, string UID)
        {
            this.Header = Header;
            this.Data = Data;
            this.UID = UID;
        }
    }
}
