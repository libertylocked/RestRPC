using System.Text;
using WebScriptHook.Framework.Plugins;

namespace WebScriptHook.Framework.BuiltinPlugins
{
    public class Echo : Plugin, IRespond
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
            StringBuilder sb = new StringBuilder();
            foreach (var arg in args)
            {
                sb.Append(arg.ToString());
            }
            return sb.ToString();
        }
    }
}
