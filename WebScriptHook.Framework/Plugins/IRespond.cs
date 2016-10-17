namespace WebScriptHook.Framework.Plugins
{
    /// <summary>
    /// Implemented by plugins that can respond to inputs
    /// </summary>
    public interface IRespond
    {
        /// <summary>
        /// Respond to a call
        /// </summary>
        /// <param name="args"></param>
        /// <returns></returns>
        object Respond(object[] args);
    }
}
