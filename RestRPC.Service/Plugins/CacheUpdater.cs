using RestRPC.Framework.Plugins;
using System;

namespace RestRPC.Service.Plugins
{
    class CacheUpdater : Plugin
    {
        public override object Respond(object[] args)
        {
            if (args.Length < 1) return false;

            SetCache("LastArgument", args[0]);
            SetCache("LastCalled", DateTime.Now);
            return true;
        }
    }
}
