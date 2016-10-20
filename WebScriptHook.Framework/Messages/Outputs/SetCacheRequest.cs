namespace WebScriptHook.Framework.Messages.Outputs
{
    /// <summary>
    /// A request to the server to set an entry in the cache map. 
    /// The server keeps a map (PluginID as key) of a map (Key as key) of data entries, 
    /// i.e. map[string]map[string]interface{}
    /// </summary>
    class SetCacheRequest : WebOutput
    {
        const char HEADER_CACHE = 'c';

        public SetCacheRequest(string PluginID, string Key, object Data)
            : base(HEADER_CACHE, new object[] { PluginID, Key, Data }, null, null)
        {
        }
    }
}
