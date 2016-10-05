using Newtonsoft.Json;

namespace WebScriptHook.Framework.Messages.Outputs
{
    /// <summary>
    /// A pulse packet can be sent by the WebScriptHook component every frame. 
    /// Can be used to sync game updates.
    /// </summary>
    class Pulse : WebOutput
    {
        const char HEADER_PULSE = 'p';

        [JsonConstructor]
        public Pulse()
            : base(HEADER_PULSE, null, null)
        { }
    }
}
