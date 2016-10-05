namespace WebScriptHook.Framework.Plugins
{
    /// <summary>
    /// Implemented by plugins that can respond to inputs
    /// </summary>
    public interface IRespond
    {
        object Respond(object[] args);
    }
}
