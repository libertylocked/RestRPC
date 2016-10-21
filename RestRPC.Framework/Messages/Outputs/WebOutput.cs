using Newtonsoft.Json;

namespace RestRPC.Framework.Messages.Outputs
{
    /// <summary>
    /// This is the format for messages client is sending to the server
    /// </summary>
    abstract class WebOutput
    {
        [JsonProperty]
        public char Header { get; set; }
        [JsonProperty]
        public object Data { get; set; }
        [JsonProperty]
        public string UID { get; private set; }
        [JsonProperty]
        public string CID { get; private set; }

        [JsonConstructor]
        public WebOutput(char Header, object Data, string UID, string CID)
        {
            this.Header = Header;
            this.Data = Data;
            this.UID = UID;
            this.CID = CID;
        }
    }
}
