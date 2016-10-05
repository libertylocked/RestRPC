using Newtonsoft.Json;

namespace WebScriptHook.Framework
{
    public class RemoteSettings
    {
        public string Protocol { get; private set; }
        public string Host { get; private set; }
        public string Port { get; private set; }
        public string Resource { get; private set; }

        [JsonConstructor]
        public RemoteSettings(string Protocol, string Host, string Port, string Resource)
        {
            this.Protocol = Protocol;
            this.Host = Host;
            this.Port = Port;
            this.Resource = Resource;
        }

        public string GetWebSocketURL()
        {
            return Protocol + "://" + Host + ":" + Port + Resource;
        }
    }
}
