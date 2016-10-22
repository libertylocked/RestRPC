using Newtonsoft.Json;
using RestRPC.Framework.Messages.Inputs;

namespace RestRPC.Framework.Messages.Outputs
{
    /// <summary>
    /// This message is sent to server as a response to a request
    /// </summary>
    class WebReturn : WebOutput
    {
        const char HEADER_RETURN = 'r';

        [JsonConstructor]
        public WebReturn(object Data, WebInput input)
            : base(HEADER_RETURN, Data, input.UID, input.CID)
        { }
    }
}
