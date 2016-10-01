using System;
using System.Collections.Generic;
using System.Linq;
using WebScriptHook.Framework.Messages;

namespace WebScriptHook.Framework
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
                    (p as ITickable).Tick();
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

        public void RegisterPlugin(Plugin plugin)
        {
            string key = plugin.PluginID;
            
            if (pluginMap.ContainsKey(key))
            {
                Logger.Log("Plugin key collision! Plugin will not be loaded: " + key, LogType.Error);
                return;
            }

            pluginMap.Add(key, plugin);
            Logger.Log("Plugin registered: " + key, LogType.Info);

            // If tickable, add to tickable list
            if (plugin is ITickable)
            {
                tickablePlugins.Add(plugin);
                Logger.Log("Found tickable plugin: " + plugin.GetType(), LogType.Info);
            }
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
