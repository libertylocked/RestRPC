using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RestRPC.Framework
{
    /// <summary>
    /// State of the socket connection in a component
    /// </summary>
    public enum ConnectionState
    {
        /// <summary>
        /// Socket is not connected
        /// </summary>
        Disconnected = 0,
        /// <summary>
        /// Socket is connecting to server
        /// </summary>
        Connecting = 1,
        /// <summary>
        /// Socket is connected to server
        /// </summary>
        Connected = 2,
        /// <summary>
        /// Socket is closing
        /// </summary>
        Closing = 3,
    }
}
