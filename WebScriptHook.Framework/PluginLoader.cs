using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;

namespace WebScriptHook.Framework
{
    class PluginLoader
    {
        /// <summary>
        /// Get Plugin subclasses from an assembly and instantiate them
        /// </summary>
        /// <param name="fileName">Filename of the assembly containing plugins</param>
        /// <returns>A list containing the plugin instances</returns>
        public static List<Plugin> LoadPluginsFromAssembly(string fileName)
        {
            var plugins = new List<Plugin>();
            try
            {
                var asm = Assembly.LoadFrom(fileName);
                var types = asm.GetTypes().Where(t => t.IsSubclassOf(typeof(Plugin)));
                foreach (var type in types)
                {
                    try
                    {
                        plugins.Add(Activator.CreateInstance(type) as Plugin);
                    }
                    catch (Exception ex)
                    {
                        // catch exceptions thrown by individual plugins in this assembly
                        Logger.Log("Failed to load " + type + " from " + fileName + ": " + ex, LogType.Error);
                    }
                }
            }
            catch (Exception ex)
            {
                Logger.Log("Failed to load assembly " + fileName + ": " + ex, LogType.Error);
            }
            return plugins;
        }

        /// <summary>
        /// Loads all plugins from a directory, recursively
        /// </summary>
        /// <param name="dirName">The directory to search for plugins in</param>
        /// <returns>The list of plugins found in the directory, all instantiated</returns>
        public static List<Plugin> LoadAllPluginsFromDir(string dirName)
        {
            var plugins = new List<Plugin>();
            try
            {
                var fileNames = Directory.GetFiles(dirName, "*.dll", SearchOption.AllDirectories);
                foreach (var fileName in fileNames)
                {
                    plugins.AddRange(LoadPluginsFromAssembly(fileName));
                }
            }
            catch (Exception ex)
            {
                Logger.Log("Failed to load plugins in directory " + dirName + ": " + ex, LogType.Error);
            }
            return plugins;
        }
    }
}
