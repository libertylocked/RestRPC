using CommandLine;
using System;
using System.IO;
using System.Threading;
using WebScriptHook.Framework;
using WebScriptHook.Framework.BuiltinPlugins;
using WebScriptHook.Framework.Plugins;
using WebScriptHook.Service.Plugins;

namespace WebScriptHook.Service
{
    class Program
    {
        static WebScriptHookComponent wshComponent;

        static void Main(string[] args)
        {
            string componentName = Guid.NewGuid().ToString();
            Uri remoteUri;

            var options = new CommandLineOptions();
            if (Parser.Default.ParseArguments(args, options))
            {
                if (!string.IsNullOrWhiteSpace(options.Name)) componentName = options.Name;
                remoteUri = new Uri(options.ServerUriString);
            }
            else
            {
                return;
            }

            wshComponent = new WebScriptHookComponent(componentName, remoteUri, TimeSpan.FromMilliseconds(30), 
                options.Username, options.Password, Console.Out, LogType.All);
            // Register custom plugins
            wshComponent.PluginManager.RegisterPlugin(new Echo());
            wshComponent.PluginManager.RegisterPlugin(new PluginList());
            wshComponent.PluginManager.RegisterPlugin(new PrintToScreen());
            // Load plugins in plugins directory if dir exists
            if (Directory.Exists("plugins"))
            {
                var plugins = PluginLoader.LoadAllPluginsFromDir("plugins", "*.dll");
                foreach (var plug in plugins)
                {
                    wshComponent.PluginManager.RegisterPlugin(plug);
                }
            }

            // Start WSH component
            wshComponent.Start();

            // TODO: Use a timer instead of Sleep
            while (true)
            {
                wshComponent.Update();
                Thread.Sleep(100);
            }
        }
    }
}
