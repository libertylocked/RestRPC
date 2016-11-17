using RestRPC.Framework.BuiltinPlugins;
using RestRPC.Framework.Exceptions;
using RestRPC.Framework.Messages.Outputs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace RestRPC.Framework.Plugins
{
    /// <summary>
    /// Plugin manager manages plugins which contains procedures that can be called
    /// </summary>
    public class PluginManager
    {
        Dictionary<string, Procedure> procedureMap = new Dictionary<string, Procedure>();
        HashSet<Plugin> tickablePlugins = new HashSet<Plugin>();
        List<Plugin> offendingPlugins = new List<Plugin>();

        internal RrpcComponent RrpcComponent
        {
            get;
            private set;
        }

        /// <summary>
        /// Gets the ID of procedures registered in this plugin manager
        /// </summary>
        public string[] ProcedureIDs
        {
            get { return procedureMap.Keys.ToArray(); }
        }

        /// <summary>
        /// Gets a list of tickable plugins registered within the plugin manager
        /// </summary>
        public Plugin[] TickablePlugins
        {
            get { return tickablePlugins.ToArray(); }
        }

        /// <summary>
        /// Constructor
        /// </summary>
        public PluginManager(RrpcComponent component)
        {
            this.RrpcComponent = component;
            Logger.Log("Plugin manager instantiated", LogType.Info);
            // Register built-in plugins
            RegisterPlugin("pluginlist", new PluginList());
            RegisterPlugin("echo", new Echo());
        }

        internal void Update()
        {
            // Tick all tickable plugins
            foreach (Plugin p in tickablePlugins)
            {
                try
                {
                    (p as IUpdate).Update();
                }
                catch (Exception ex)
                {
                    offendingPlugins.Add(p);
                    Logger.Log(ex, LogType.Error);
                }
            }

            // Unregister plugins marked in offending plugins list
            for (int i = offendingPlugins.Count - 1; i >= 0; i--)
            {
                UnregisterPlugin(offendingPlugins[i]);
                offendingPlugins.RemoveAt(i);
            }
        }

        internal object Dispatch(string procedureID, object[] args)
        {
            Procedure method;
            // Can only dispatch if the procedureID is found
            if (procedureMap.TryGetValue(procedureID, out method))
            {
                return method(args);
            }
            else
            {
                // Procedure not found
                throw new MethodNotFoundException(procedureID);
            }
        }

        internal void SetCache(string key, object value)
        {
            var cacheUpdateMsg = new CacheUpdateMessage(new CacheObject(key, value));
            if (RrpcComponent.ConnectionState == ConnectionState.Connected)
            {
                RrpcComponent.EnqueueOutMessage(cacheUpdateMsg);
            }
        }

        internal Task RunOnUpdateThread(Action action)
        {
            return RrpcComponent.RunOnUpdateThread(action);
        }

        /// <summary>
        /// Registers a plugin. If plugin is tickable, it's also added to the tickable list
        /// </summary>
        /// <param name="procedureID">ID of the procedure. Must be unique or register will fail</param>
        /// <param name="plugin">The plugin to be registered</param>
        /// <returns>True if plugin is successfully registered</returns>
        public bool RegisterPlugin(string procedureID, Plugin plugin)
        {
            // Add plugin's procedure
            if (!RegisterProcedure(procedureID, plugin.Respond))
            {
                return false;
            }

            // If tickable, add to tickable list
            if (plugin is IUpdate)
            {
                if (tickablePlugins.Add(plugin))
                {
                    Logger.Log("Added to tickable list: " + plugin.GetType(), LogType.Info);
                }
                else
                {
                    // Allow the same plugin instance to be added twice (for alias purposes)
                    Logger.Log("Plugin is already in tickable list: " + plugin.GetType(), LogType.Info);
                }
            }

            // Set plugin manager property of the plugin
            plugin.PluginManager = this;

            return true;
        }

        /// <summary>
        /// Unregisters the plugin's procedure and removes it from being ticked if tickable
        /// </summary>
        /// <param name="plugin">The plugin instance to be unregistered. It must be already registered</param>
        public void UnregisterPlugin(Plugin plugin)
        {
            // Remove this plugin's procedure from the procedure map
            // This also removes all the aliases
            foreach (var item in procedureMap.Where(pair => pair.Value == plugin.Respond).ToList())
            {
                procedureMap.Remove(item.Key);
            }

            // If this plugin is tickable, remove it from tickable plugins list
            if (tickablePlugins.Contains(plugin))
            {
                tickablePlugins.Remove(plugin);
            }

            plugin.PluginManager = null;
        }

        /// <summary>
        /// Registers a procedure
        /// </summary>
        /// <param name="procedureID">ID of the procedure</param>
        /// <param name="procedure"></param>
        /// <returns></returns>
        public bool RegisterProcedure(string procedureID, Procedure procedure)
        {
            if (procedureMap.ContainsKey(procedureID))
            {
                Logger.Log("Procedure key collision! " + procedureID, LogType.Error);
                return false;
            }

            // Add procedure to procedure map
            procedureMap.Add(procedureID, procedure);
            Logger.Log("Procedure registered: " + procedureID, LogType.Info);
            return true;
        }

        /// <summary>
        /// Unregisters a procedure 
        /// If this is used to unregister a tickable plugin's procedure, the plugin itself will not be unticked
        /// </summary>
        /// <param name="procedureID"></param>
        public void UnregisterProcedure(string procedureID)
        {
            if (procedureMap.ContainsKey(procedureID))
            {
                procedureMap.Remove(procedureID);
            }

            Logger.Log("Procedure unregistered: " + procedureID, LogType.Info);
        }
    }
}
