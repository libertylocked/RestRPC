using Newtonsoft.Json;

namespace RestRPC.Framework.Messages.Outputs
{
    /// <summary>
    /// A pulse packet can be sent by the RestRPC component every frame. 
    /// Can be used to sync game updates.
    /// </summary>
    class Pulse : WebOutput
    {
        const char HEADER_PULSE = 'p';

        [JsonConstructor]
        public Pulse()
            : base(HEADER_PULSE, null, null, null)
        { }
    }
}
