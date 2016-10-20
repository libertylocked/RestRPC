using RestRPC.Framework.Plugins;

namespace RestRPC.Framework.BuiltinPlugins
{
    /// <summary>
    /// A built-in plugin of RestRPC
    /// Handles "pluginlist" request and returns the list of plugins loaded
    /// </summary>
    internal class PluginList : Plugin
    {
        /// <summary>
        /// Gets the list of plugins loaded in the plugin manager
        /// </summary>
        /// <param name="args">Not used</param>
        /// <returns>An array of ID strings of plugins loaded</returns>
        public override object Respond(object[] args)
        {
            return PluginManager.PluginIDs;
        }
    }
}
