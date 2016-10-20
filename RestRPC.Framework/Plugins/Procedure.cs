using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RestRPC.Framework.Plugins
{
    /// <summary>
    /// Respond to a remote procedure call
    /// </summary>
    /// <param name="args"></param>
    /// <returns></returns>
    public delegate object Procedure(object[] args);
}
