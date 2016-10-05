using System;
using System.Threading;
using WebScriptHook.Framework;
using WebScriptHook.Framework.BuiltinPlugins;
using WebScriptHook.Terminal.Plugins;

namespace WebScriptHook.Terminal
{
    class Program
    {
        static WebScriptHookComponent wshComponent;

        static void Main(string[] args)
        {
            string componentName = "testbench"; //Guid.NewGuid().ToString();
            wshComponent = new WebScriptHookComponent(componentName, new RemoteSettings("ws", "localhost", "25555", "/componentws"));
            // Register custom plugins
            wshComponent.PluginManager.RegisterPlugin(new Echo());
            wshComponent.PluginManager.RegisterPlugin(new PluginList());
            wshComponent.PluginManager.RegisterPlugin(new PrintToScreen());
            // Start WSH component
            wshComponent.Start();

            // Print all registered plugins
            Console.WriteLine("Registered plugins on \"" + wshComponent.Name + "\":");
            var pluginIDs = wshComponent.PluginManager.PluginIDs;
            foreach (var pluginID in pluginIDs)
            {
                Console.WriteLine(pluginID);
            }

            // TODO: Use a timer instead of Sleep
            while (true)
            {
                wshComponent.Update();
                Thread.Sleep(20);
            }
        }
    }
}
