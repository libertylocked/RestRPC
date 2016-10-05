namespace WebScriptHook.Framework.Plugins
{
    /// <summary>
    /// Implemented by plugins that need to be ticked every frame
    /// </summary>
    public interface IUpdate
    {
        void Update();
    }
}
