using WebScriptHook.Framework.Plugins;

namespace WebScriptHook.Framework.BuiltinPlugins
{
    /// <summary>
    /// A built-in plugin of WebScriptHook
    /// Handles "pluginlist" request and returns the list of plugins loaded
    /// </summary>
    class PluginList : Plugin, IRespond
    {
        protected internal override string PluginIDImpl
        {
            get
            {
                return "pluginlist";
            }
        }

        /// <summary>
        /// Gets the list of plugins loaded in the plugin manager
        /// </summary>
        /// <param name="args">Not used<param>
        /// <returns>An array of ID strings of plugins loaded</returns>
        public object Respond(object[] args)
        {
            return PluginManager.Instance.PluginIDs;
        }
    }
}
