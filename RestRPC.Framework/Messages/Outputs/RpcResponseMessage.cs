namespace RestRPC.Framework.Messages.Outputs
{
    /// <summary>
    /// This message is sent to server as a response to a request
    /// </summary>
    class RpcResponseMessage : OutMessage
    {
        public RpcResponseMessage(ResponseObject ResponseObject)
            : base("", ResponseObject)
        { }
    }
}
