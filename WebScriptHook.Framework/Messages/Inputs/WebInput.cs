﻿using Newtonsoft.Json;
using System.Text;

namespace WebScriptHook.Framework.Messages.Inputs
{
    /// <summary>
    /// This is the format of messages client is receiving from the remote server
    /// </summary>
    class WebInput
    {
        /// <summary>
        /// Gets the header of the request. 
        /// Header is currently unused
        /// </summary>
        public char Header { get; set; }

        /// <summary>
        /// Gets the command of the request. 
        /// Cmd is the command this input request is calling. It is the ID of the plugin 
        /// the request wants to call
        /// </summary>
        public string Cmd { get; set; }

        /// <summary>
        /// Gets the arguments in this request. 
        /// Arguments supplied to the plugin
        /// </summary>
        public object[] Args { get; set; }

        /// <summary>
        /// Gets the UID of this request.
        /// </summary>
        public string UID { get; private set; }

        [JsonConstructor]
        public WebInput(char Header, string Cmd, object[] Args, string UID)
        {
            this.Header = Header;
            this.Cmd = Cmd;
            this.Args = Args;
            this.UID = UID;
        }

        public override string ToString()
        {
            return "Header: " + Header + ", Cmd: " + Cmd + ", Args: [" + 
                ((Args == null) ? "" : string.Join(",", Args)) +
                "], UID: " + UID;
        }
    }
}
