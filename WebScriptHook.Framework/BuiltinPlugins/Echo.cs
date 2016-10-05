using WebScriptHook.Framework.Plugins;

namespace WebScriptHook.Framework.BuiltinPlugins
{
    class Echo : Plugin, IRespond
    {
        protected internal override string PluginIDImpl
        {
            get
            {
                return "echo";
            }
        }

        public object Respond(object[] args)
        {
            return args;
        }
    }
}
