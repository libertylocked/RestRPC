using Newtonsoft.Json;

namespace WebScriptHook.Framework.Messages
{
    /// <summary>
    /// A pulse packet is sent by the client every frame. 
    /// UID is not needed in this message
    /// </summary>
    class WebPulse : WebOutput
    {
        public string ClientName { get; private set; }

        const string HEADER_PULSE = "pulse";

        [JsonConstructor]
        public WebPulse(string ClientName)
            :base(HEADER_PULSE, ClientName, "")
        {
            this.ClientName = ClientName;
        }
    }
}
