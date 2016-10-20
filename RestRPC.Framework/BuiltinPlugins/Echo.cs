using System.Text;
using RestRPC.Framework.Plugins;

namespace RestRPC.Framework.BuiltinPlugins
{
    internal class Echo : Plugin
    {
        /// <summary>
        /// Returns the input args concatenated as a string
        /// </summary>
        /// <param name="args"></param>
        /// <returns></returns>
        public override object Respond(object[] args)
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
