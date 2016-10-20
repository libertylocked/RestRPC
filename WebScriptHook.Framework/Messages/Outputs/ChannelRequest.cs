namespace WebScriptHook.Framework.Messages.Outputs
{
    /// <summary>
    /// Sent by the component to request a channel for itself on the server. 
    /// This tells the server the name of this component, as well as the maximum number of requests 
    /// the component can handle per tick. 
    /// Once the server receives this message, a channel will be created, registered under this component's name. 
    /// Inputs sent by web clients will then be delivered to this component. 
    /// </summary>
    class ChannelRequest : WebOutput
    {
        const char HEADER_CHANNEL = 'n';

        public ChannelRequest(string ComponentName, int InputQueueLimit)
            : base(HEADER_CHANNEL, new object[] { ComponentName, InputQueueLimit }, null, null)
        {
        }
    }
}
