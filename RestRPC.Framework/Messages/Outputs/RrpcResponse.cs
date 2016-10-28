using Newtonsoft.Json;
using RestRPC.Framework.Messages.Inputs;

namespace RestRPC.Framework.Messages.Outputs
{
    /// <summary>
    /// This message is sent to server as a response to a request
    /// </summary>
    class RrpcResponse : OutMessage
    {
        public RrpcResponse(ResponseObject ResponseObject)
            : base("", ResponseObject)
        { }
    }
}
