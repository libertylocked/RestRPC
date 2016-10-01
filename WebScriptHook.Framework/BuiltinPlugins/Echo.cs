namespace WebScriptHook.Framework.BuiltinPlugins
{
    class Echo : Plugin
    {
        protected internal override string PluginIDImpl
        {
            get
            {
                return "echo";
            }
        }

        public override object Respond(object[] args)
        {
            return args;
        }
    }
}
