using System;
using System.Collections.Generic;
using System.Linq;
using WebScriptHook.Framework.Messages.Outputs;

namespace WebScriptHook.Framework.Plugins
{
    public class PluginManager
    {
        static PluginManager instance;

        Dictionary<string, Plugin> pluginMap = new Dictionary<string, Plugin>();
        HashSet<Plugin> tickablePlugins = new HashSet<Plugin>();
        List<Plugin> offendingPlugins = new List<Plugin>();

        internal static PluginManager Instance
        {
            get
            {
                if (instance == null) CreateInstance();
                return instance;
            }
        }

        internal string[] PluginIDs
        {
            get { return pluginMap.Keys.ToArray(); }
        }

        /// <summary>
        /// Constructor
        /// </summary>
        private PluginManager() { }

        /// <summary>
        /// Creates an instance of Plugin Manager
        /// </summary>
        /// <returns></returns>
        internal static PluginManager CreateInstance()
        {
            instance = new PluginManager();
            Logger.Log("Plugin manager instantiated", LogType.Info);
            return instance;
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
            // Can only dispatch if the pluginID is found and the plugin implements IRespond
            if (pluginMap.TryGetValue(pluginId, out callee) && (callee is IRespond))
            {
                return (callee as IRespond).Respond(args);
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

        public bool RegisterPlugin(Plugin plugin)
        {
            string key = plugin.PluginID;
            
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

            return true;
        }

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
