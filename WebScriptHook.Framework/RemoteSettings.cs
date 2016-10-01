using Newtonsoft.Json;

namespace WebScriptHook.Framework
{
    public class RemoteSettings
    {
        public string Host { get; private set; }
        public string Port { get; private set; }
        public string Resource { get; private set; }

        [JsonConstructor]
        public RemoteSettings(string Host, string Port, string Resource)
        {
            this.Host = Host;
            this.Port = Port;
            this.Resource = Resource;
        }

        public string GetWebSocketURL()
        {
            return "ws://" + Host + ":" + Port + Resource;
        }
    }
}
