using System;
using WebScriptHook.Framework.Plugins;
using WebScriptHook.Framework.Messages.Outputs;

namespace WebScriptHook.Terminal.Plugins
{
    class PrintToScreen : Plugin, IRespond
    {
        public object Respond(object[] args)
        {
            foreach (object arg in args)
            {
                Console.Write(arg.ToString());
            }
            Console.Write(Environment.NewLine);

            // This plugin does not return anything to the requester
            return new NoOutput();
        }
    }
}
