using RestRPC.Framework.BuiltinPlugins;
using RestRPC.Framework.Messages.Outputs;
using System;
using System.Collections.Generic;
using System.Linq;

namespace RestRPC.Framework.Plugins
{
    /// <summary>
    /// Plugin manager manages plugins which contains procedures that can be called
    /// </summary>
    public class PluginManager
    {
        Dictionary<string, Plugin> pluginMap = new Dictionary<string, Plugin>();
        HashSet<Plugin> tickablePlugins = new HashSet<Plugin>();
        List<Plugin> offendingPlugins = new List<Plugin>();

        /// <summary>
        /// Gets the IDs of plugins registered in this plugin manager
        /// </summary>
        public string[] PluginIDs
        {
            get { return pluginMap.Keys.ToArray(); }
        }

        /// <summary>
        /// Constructor
        /// </summary>
        public PluginManager()
        {
            Logger.Log("Plugin manager instantiated", LogType.Info);
            // Register built-in plugins
            RegisterPlugin(new PluginList(), "pluginlist");
            RegisterPlugin(new Echo(), "echo");
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

        internal object Dispatch(string pluginId, object[] args)
        {
            Plugin callee;
            // Can only dispatch if the pluginID is found
            if (pluginMap.TryGetValue(pluginId, out callee))
            {
                return callee.Respond(args);
            }
            else
            {
                // Plugin not found - return nothing
                return new NoOutput();
            }
        }

        internal void SetCache(string pluginId, string key, object value)
        {
            WebOutput cacheRequestMessage = new SetCacheRequest(pluginId, key, value);

            // TODO: This message needs to be added to the output message queue
            throw new NotImplementedException();
        }

        /// <summary>
        /// Registers a plugin
        /// </summary>
        /// <param name="plugin">The plugin to be registered</param>
        /// <param name="key">ID of the procedure. Must be unique or register will fail</param>
        /// <returns>True if plugin is successfully registered</returns>
        public bool RegisterPlugin(Plugin plugin, string key)
        {            
            if (pluginMap.ContainsKey(key))
            {
                Logger.Log("Plugin key collision! Plugin will not be loaded: " + key, LogType.Error);
                return false;
            }

            pluginMap.Add(key, plugin);
            Logger.Log("Plugin registered: " + key, LogType.Info);

            // If tickable, add to tickable list
            if (plugin is IUpdate)
            {
                tickablePlugins.Add(plugin);
                Logger.Log("Found tickable plugin: " + plugin.GetType(), LogType.Info);
            }

            // Set plugin manager property of the plugin
            plugin.PluginManager = this;
            // Set plugin id property of this plugin
            plugin.PluginID = key;

            return true;
        }

        /// <summary>
        /// Unregisters a plugin
        /// </summary>
        /// <param name="plugin">The plugin instance to be unregistered. It must be already registered</param>
        public void UnregisterPlugin(Plugin plugin)
        {
            // Drop from tickables
            if (plugin != null && tickablePlugins.Contains(plugin))
            {
                tickablePlugins.Remove(plugin);
            }

            // Drop from plugin map
            foreach (var item in pluginMap.Where(pair => pair.Value == plugin).ToList())
            {
                pluginMap.Remove(item.Key);
            }
        }
    }
}
