namespace RestRPC.Framework.Messages.Outputs
{
    /// <summary>
    /// A request to the server to set an entry in the cache map
    /// </summary>
    class SetCacheRequest : WebOutput
    {
        const char HEADER_CACHE = 'c';

        public SetCacheRequest(string Key, object Data)
            : base(HEADER_CACHE, new object[] { Key, Data }, null, null)
        {
        }
    }
}
