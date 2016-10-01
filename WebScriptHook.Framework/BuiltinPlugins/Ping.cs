using System;

namespace WebScriptHook.Framework.BuiltinPlugins
{
    class Ping : Plugin
    {
        protected internal override string PluginIDImpl
        {
            get
            {
                return "ping";
            }
        }

        public override object Respond(object[] args)
        {
            return "pong";
        }
    }
}
