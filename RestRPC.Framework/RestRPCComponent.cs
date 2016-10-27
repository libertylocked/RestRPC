using Newtonsoft.Json;
using System;
using System.Collections.Concurrent;
using System.IO;
using RestRPC.Framework.Messages.Inputs;
using RestRPC.Framework.Messages.Outputs;
using RestRPC.Framework.Plugins;
using RestRPC.Framework.Serialization;
using WebSocketSharp;
using WebSocketSharp.Net;

namespace RestRPC.Framework
{
    /// <summary>
    /// RestRPCComponent communicates with an RRPC server and handles in/out messages
    /// </summary>
    public class RestRPCComponent
    {
        const int CHANNEL_SIZE = 50;

        WebSocket ws;
        DateTime lastPollTime = DateTime.Now;

        ConcurrentQueue<WebInput> inputQueue = new ConcurrentQueue<WebInput>();
        ConcurrentQueue<WebOutput> outputQueue = new ConcurrentQueue<WebOutput>();

        JsonSerializerSettings outSerializerSettings = new JsonSerializerSettings
        {
            ContractResolver = new WritablePropertiesOnlyResolver(),
            NullValueHandling = NullValueHandling.Ignore
        };

        /// <summary>
        /// Gets the name of this RestRPC component
        /// </summary>
        public string Name
        {
            get;
            private set;
        }

        /// <summary>
        /// Gets information on the remote this component is connecting to
        /// </summary>
        public Uri RemoteUri
        {
            get;
            private set;
        }

        /// <summary>
        /// Gets the rate the component polls messages from server
        /// </summary>
        public TimeSpan PollingRate
        {
            get;
            private set;
        }

        /// <summary>
        /// Gets an indicator whether the component is running network updates.
        /// </summary>
        public bool IsRunning
        {
            get;
            private set;
        } = false;

        /// <summary>
        /// Gets the state of WebSocket connection
        /// </summary>
        public ConnectionState ConnectionState
        {
            get;
            private set;
        } = ConnectionState.Disconnected;

        /// <summary>
        /// Gets the instance of PluginManager in this component
        /// </summary>
        public PluginManager PluginManager
        {
            get;
            set;
        }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="componentName">The name of this RestRPC component</param>
        /// <param name="remoteUri">Remote server settings</param>
        /// <param name="pollingRate">Rate to poll messages from server</param>
        /// <param name="username">Username for HTTP auth</param>
        /// <param name="password">Password for HTTP auth</param>
        public RestRPCComponent(string componentName, Uri remoteUri, TimeSpan pollingRate, 
            string username, string password)
        : this(componentName, remoteUri, pollingRate, username, password, null, LogType.None)
        { }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="componentName">The name of this RestRPC component</param>
        /// <param name="remoteUri">Remote server settings</param>
        /// <param name="pollingRate">Rate to poll messages from server</param>
        /// <param name="username">Username for HTTP auth</param>
        /// <param name="password">Password for HTTP auth</param>
        /// <param name="logWriter">Log writer</param>
        /// <param name="logLevel">Level of logging</param>
        public RestRPCComponent(string componentName, Uri remoteUri, TimeSpan pollingRate, 
            string username, string password, TextWriter logWriter, LogType logLevel)
        {
            this.Name = componentName;
            this.RemoteUri = remoteUri;
            this.PollingRate = pollingRate;

            Logger.Writer = logWriter;
            Logger.LogLevel = logLevel;

            // Set up network worker, which exchanges data between plugin and server
            ws = new WebSocket(remoteUri.ToString());
            ws.OnMessage += WS_OnMessage;
            ws.OnOpen += WS_OnOpen;
            ws.OnClose += WS_OnClose;
            ws.OnError += WS_OnError;
            // HTTP basic auth
            ws.SetCredentials(username, password, true);

            // Create plugin manager instance
            PluginManager = new PluginManager();
        }

        /// <summary>
        /// Starts connection to the remote server. 
        /// The component will always attempt to reconnect if disconnected while it is running
        /// </summary>
        public void Start()
        {
            if (!IsRunning)
            {
                // Set "svcName" cookie so the server knows who we are
                ws.SetCookie(new Cookie("svcName", Name));
                IsRunning = true;
            }
        }

        /// <summary>
        /// Stops connection to the remote server. 
        /// All unprocessed inputs and unsent outputs will be discarded
        /// </summary>
        public void Stop()
        {
            if (IsRunning)
            {
                // Clear queues
                inputQueue = new ConcurrentQueue<WebInput>();
                outputQueue = new ConcurrentQueue<WebOutput>();

                IsRunning = false;
            }
        }

        /// <summary>
        /// Updates RestRPCComponent. Should be called on every tick.
        /// </summary>
        public void Update()
        {
            // Tick plugin manager
            PluginManager.Update();

            // Process input messages
            ProcessInputMessages();

            // Send messages over websocket
            NetworkUpdate();
        }

        private void NetworkUpdate()
        {
            // Only perform network update once per polling interval
            if (DateTime.Now - lastPollTime < PollingRate) return;
            lastPollTime = DateTime.Now;

            // Connect ws if running and ws not connected
            if (IsRunning && ConnectionState == ConnectionState.Disconnected)
            {
                ws.ConnectAsync();
                ConnectionState = ConnectionState.Connecting;
            }

            // Disconnect if not running and ws open
            if (!IsRunning && ConnectionState == ConnectionState.Connected)
            {
                ws.CloseAsync(CloseStatusCode.Normal);
                ConnectionState = ConnectionState.Closing;
            }

            // Send messages on socket when connection is open
            if (ConnectionState == ConnectionState.Connected)
            {
                ProcessOutputMessages();
            }
        }

        private void ProcessInputMessages()
        {
            WebInput input;
            while (inputQueue.TryDequeue(out input))
            {
                try
                {
                    // Process this message
                    Logger.Log("Executing " + input.ToString(), LogType.Debug);
                    object retVal = PluginManager.Dispatch(input.Cmd, input.Args);
                    // Only return real values. Do not return NoOutput messages
                    if (retVal == null || retVal.GetType() != typeof(NoOutput))
                    {
                        outputQueue.Enqueue(new WebReturn(retVal, input));
                    }
                }
                catch (Exception ex)
                {
                    Logger.Log(ex.ToString(), LogType.Error);
                }
            }
        }

        private void ProcessOutputMessages()
        {
            // Output messages can only be processed when websocket connection is open
            if (ws.ReadyState != WebSocketState.Open)
            {
                throw new Exception("Cannot process output messages when WebSocket connection is not open!");
            }

            // Send output data
            WebOutput output;
            while (outputQueue.TryDequeue(out output))
            {
                // Serialize the object to JSON then send back to server.
                try
                {
                    ws.SendAsync(JsonConvert.SerializeObject(output, outSerializerSettings), null);
                }
                catch (Exception sendExc)
                {
                    Logger.Log(sendExc.ToString(), LogType.Error);
                }
            }
        }

        private void WS_OnMessage(object sender, MessageEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(e.Data)) return;
            WebInput input = JsonConvert.DeserializeObject<WebInput>(e.Data);
            if (input != null)
            {
                inputQueue.Enqueue(input);
            }
        }

        private void WS_OnOpen(object sender, EventArgs e)
        {
            ConnectionState = ConnectionState.Connected;
            Logger.Log("WebSocket connection established: " + ws.Url, LogType.Info);
        }

        private void WS_OnClose(object sender, CloseEventArgs e)
        {
            // This can occur either when socket connect fails or socket disconnects while connected
            if (ConnectionState != ConnectionState.Connecting)
            {
                Logger.Log("WebSocket connection closed: " + ws.Url, LogType.Info);
            }
            ConnectionState = ConnectionState.Disconnected;
        }

        private void WS_OnError(object sender, WebSocketSharp.ErrorEventArgs e)
        {
            Logger.Log("WebSocket error: " + e.Message, LogType.Error);
        }
    }
}
