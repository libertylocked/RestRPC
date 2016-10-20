using System;
using WebScriptHook.Framework.Plugins;

namespace WebScriptHook.Service.Plugins
{
    class PrintToScreen : Plugin
    {
        public override object Respond(object[] args)
        {
            foreach (object arg in args)
            {
                Console.Write(arg.ToString());
            }
            Console.Write(Environment.NewLine);

            // Return true so that our client will be notified that the operation succeeded
            return true;
        }
    }
}
