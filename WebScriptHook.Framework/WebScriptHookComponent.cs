using Newtonsoft.Json;
using System;
using System.Collections.Concurrent;
using System.Threading;
using WebScriptHook.Framework.Messages.Inputs;
using WebScriptHook.Framework.Messages.Outputs;
using WebScriptHook.Framework.Plugins;
using WebScriptHook.Framework.Serialization;
using WebSocketSharp;

namespace WebScriptHook.Framework
{
    public class WebScriptHookComponent
    {
        static AutoResetEvent networkWaitHandle = new AutoResetEvent(false);
        WebSocket ws;
        Thread networkThread;

        ConcurrentQueue<WebInput> inputQueue = new ConcurrentQueue<WebInput>();
        ConcurrentQueue<WebOutput> outputQueue = new ConcurrentQueue<WebOutput>();

        JsonSerializerSettings outSerializerSettings = new JsonSerializerSettings { ContractResolver = new WritablePropertiesOnlyResolver() };

        /// <summary>
        /// Gets the name of this WebScriptHook component
        /// </summary>
        public string Name
        {
            get;
            private set;
        }

        /// <summary>
        /// Gets information on the remote this component is connecting to
        /// </summary>
        public RemoteSettings RemoteSettings
        {
            get;
            private set;
        }

        public PluginManager PluginManager
        {
            get { return PluginManager.Instance; }
        }

        public bool IsRunning
        {
            get;
            private set;
        } = false;

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="componentName">The name of this WebScriptHook component</param>
        /// <param name="remoteSettings">Remote server settings</param>
        public WebScriptHookComponent(string componentName, RemoteSettings remoteSettings)
        : this(componentName, remoteSettings, null, LogType.None)
        { }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="componentName">The name of this WebScriptHook component</param>
        /// <param name="remoteSettings">Remote server settings</param>
        /// <param name="logFile">Log filename</param>
        /// <param name="logLevel">Level of logging</param>
        public WebScriptHookComponent(string componentName, RemoteSettings remoteSettings, string logFile, LogType logLevel)
        {
            this.Name = componentName;
            this.RemoteSettings = remoteSettings;

            Logger.FileName = logFile;
            Logger.LogLevel = logLevel;

            // Set up network worker, which exchanges data between plugin and server
            ws = new WebSocket(remoteSettings.GetWebSocketURL());
            ws.OnMessage += WS_OnMessage;

            // Create plugin manager instance
            PluginManager.CreateInstance();
        }

        /// <summary>
        /// Creates connection to the remote server
        /// </summary>
        public void Start()
        {
            if (!IsRunning)
            {
                networkThread = new Thread(Worker_OnTick);
                networkThread.IsBackground = true;
                networkThread.Start();
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
                // Abort network thread and close ws connection
                networkThread.Abort();
                ws.Close();
                // Clear queues
                inputQueue = new ConcurrentQueue<WebInput>();
                outputQueue = new ConcurrentQueue<WebOutput>();
            }
        }

        /// <summary>
        /// Updates WebScriptHookComponent. Should be called on every tick.
        /// </summary>
        public void Update()
        {
            // Signal network worker
            networkWaitHandle.Set();

            // Tick plugin manager
            PluginManager.Instance.Update();

            // Process input messages
            ProcessInputMessages();
        }

        private void ProcessInputMessages()
        {
            WebInput input;
            while (inputQueue.TryDequeue(out input))
            {
                try
                {
                    // Process this message
                    Logger.Log("Executing " + input.Cmd, LogType.Debug);
                    object retVal = PluginManager.Instance.Dispatch(input.Cmd, input.Args);
                    // Only return real values. Do not return NoOutput messages
                    if (retVal.GetType() != typeof(NoOutput))
                    {
                        outputQueue.Enqueue(new WebReturn(retVal, input.UID));
                    }
                }
                catch (Exception ex)
                {
                    Logger.Log(ex.ToString(), LogType.Error);
                }
            }
        }

        private void Worker_OnTick()
        {
            while (true)
            {
                // Wait until a tick happens
                networkWaitHandle.WaitOne();

                try
                {
                    // Check if connection is alive. If not, attempt to connect to server
                    // WS doesn't throw exceptions when connection fails or unconnected
                    if (!ws.IsAlive) ws.Connect();
                    // Send output data
                    WebOutput output;
                    while (outputQueue.TryDequeue(out output))
                    {
                        // Serialize the object to JSON then send back to server.
                        // Word of warning: If some plugin attempts to send an object that cannot be seralized, 
                        // this iteration of worker update will be terminated. Whatever is left on the queue may be unsent.
                        ws.Send(JsonConvert.SerializeObject(output, outSerializerSettings));
                    }
                }
                catch (Exception exc)
                {
                    Logger.Log(exc.ToString(), LogType.Error);
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
    }
}
