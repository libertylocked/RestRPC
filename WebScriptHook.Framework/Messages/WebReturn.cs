using Newtonsoft.Json;

namespace WebScriptHook.Framework.Messages
{
    /// <summary>
    /// This message is sent to server as a response to a request
    /// </summary>
    class WebReturn : WebOutput
    {
        const string HEADER_RETURN = "return";

        [JsonConstructor]
        public WebReturn(object Data, string UID)
            : base(HEADER_RETURN, Data, UID)
        { }
    }
}
