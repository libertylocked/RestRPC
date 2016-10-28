using CommandLine;
using System;
using System.IO;
using System.Threading;
using RestRPC.Framework;
using RestRPC.Framework.Plugins;
using RestRPC.Service.Plugins;

namespace RestRPC.Service
{
    class Program
    {
        static RrpcComponent wshComponent;

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

            wshComponent = new RrpcComponent(componentName, remoteUri, TimeSpan.FromMilliseconds(30), 
                options.Username, options.Password, Console.Out, LogType.All);
            // Register custom plugins and procedures
            wshComponent.PluginManager.RegisterPlugin("print", new PrintToScreen());
            wshComponent.PluginManager.RegisterProcedure("osversion", (inputArgs) => { return Environment.OSVersion.VersionString; });
            wshComponent.PluginManager.RegisterProcedure("invalidop", (inputArgs) => { throw new InvalidOperationException("This exception is a test!"); });
            // Load plugins in plugins directory if dir exists
            if (Directory.Exists("plugins"))
            {
                var plugins = PluginLoader.LoadAllPluginsFromDir("plugins", "*.dll");
                foreach (var plug in plugins)
                {
                    wshComponent.PluginManager.RegisterPlugin(plug.GetType().FullName, plug);
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
