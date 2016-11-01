namespace RestRPC.Framework.Messages.Outputs
{
    /// <summary>
    /// This message is sent to the server as a request to update this service's cache space. 
    /// The header for this message is "c"
    /// </summary>
    class CacheUpdateMessage : OutMessage
    {
        public CacheUpdateMessage(CacheObject CacheObject)
            : base("c", CacheObject)
        { }
    }
}
