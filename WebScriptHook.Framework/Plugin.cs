namespace WebScriptHook.Framework
{
    /// <summary>
    /// A plugin extends the request types WebScriptHook can handle
    /// </summary>
    public abstract class Plugin
    {
        protected internal virtual string PluginIDImpl
        {
            get
            {
                // This allows internal builtin plugins to override their ID string
                return this.GetType().Assembly.GetName().Name + "." + this.GetType().FullName;
            }
        }

        /// <summary>
        /// Gets the callable ID of the plugin. ID string is 
        /// in the format of assemblyname.namespace.classname
        /// </summary>
        public string PluginID
        {
            get { return PluginIDImpl; }
        }

        /// <summary>
        /// Answers a call to this plugin
        /// </summary>
        /// <param name="args">Arguments passed with the call</param>
        /// <returns>Object to be returned to caller</returns>
        public abstract object Respond(object[] args);

        /// <summary>
        /// Calls another plugin loaded in Plugin Manager
        /// </summary>
        /// <param name="targetID">Callee's plugin ID</param>
        /// <param name="args">Arguments passed with the call</param>
        /// <returns>Object returned by the callee</returns>
        protected object CallPlugin(string targetID, object[] args)
        {
            return PluginManager.Instance.Dispatch(targetID, args);
        }
    }
}
