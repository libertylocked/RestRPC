using System;
using System.Threading.Tasks;

namespace RestRPC.Framework.Plugins
{
    /// <summary>
    /// A plugin extends the request types RestRPC can handle
    /// </summary>
    public abstract class Plugin
    {
        internal PluginManager PluginManager
        {
            get;
            set;
        }

        /// <summary>
        /// Schedule a task to be run on the Update thread. 
        /// This ensures the action will be run when component updates
        /// </summary>
        /// <param name="action"></param>
        /// <returns></returns>
        protected Task RunOnUpdateThread(Action action)
        {
            return PluginManager.RunOnUpdateThread(action);
        }

        /// <summary>
        /// Dispatch another plugin loaded in Plugin Manager
        /// </summary>
        /// <param name="targetID">Callee's plugin ID</param>
        /// <param name="args">Arguments passed with the call</param>
        /// <returns>Object returned by the callee</returns>
        protected object Dispatch(string targetID, object[] args)
        {
            return PluginManager.Dispatch(targetID, args);
        }

        /// <summary>
        /// Requests the key-value pair be cached in this plugin's cache map, on the server
        /// </summary>
        /// <param name="key">Key of the entry in the cache map</param>
        /// <param name="value">Value of the entry in the cache map</param>
        protected void SetCache(string key, object value)
        {
            PluginManager.SetCache(key, value);
        }

        /// <summary>
        /// Respond to a call to this plugin's procedure
        /// </summary>
        /// <param name="args"></param>
        /// <returns></returns>
        public abstract object Respond(object[] args);
    }
}
