using Newtonsoft.Json;

namespace WebScriptHook.Framework.Messages.Outputs
{
    /// <summary>
    /// A pulse packet can be sent by the WebScriptHook component every frame. 
    /// Can be used to sync game updates
    /// UID is not needed in this message
    /// </summary>
    class WebPulse : WebOutput
    {
        const char HEADER_PULSE = 'p';

        [JsonConstructor]
        public WebPulse(object Data)
            : base(HEADER_PULSE, Data, "")
        { }
    }
}
