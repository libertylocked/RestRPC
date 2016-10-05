using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WebScriptHook.Framework;

namespace WebScriptHook.Terminal
{
    class Program
    {
        static WebScriptHookComponent wshComponent;

        static void Main(string[] args)
        {
            string componentName = Guid.NewGuid().ToString();
            wshComponent = new WebScriptHookComponent(componentName, new RemoteSettings("localhost", "25555", "/pushws"));
        }
    }
}
