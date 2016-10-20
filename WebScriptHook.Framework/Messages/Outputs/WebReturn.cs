using Newtonsoft.Json;
using WebScriptHook.Framework.Messages.Inputs;

namespace WebScriptHook.Framework.Messages.Outputs
{
    /// <summary>
    /// This message is sent to server as a response to a request
    /// </summary>
    class WebReturn : WebOutput
    {
        const char HEADER_RETURN = 'r';

        [JsonConstructor]
        public WebReturn(object Data, WebInput input)
            : base(HEADER_RETURN, new object[] { Data }, input.UID, input.CID)
        { }
    }
}
