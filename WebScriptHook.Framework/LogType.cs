namespace WebScriptHook.Framework
{
    /// <summary>
    /// Type of a log message
    /// </summary>
    public enum LogType
    {
        /// <summary>
        /// No type
        /// </summary>
        None = 0,
        /// <summary>
        /// An info message
        /// </summary>
        Info = 1,
        /// <summary>
        /// A warning message
        /// </summary>
        Warning = 2,
        /// <summary>
        /// An error message
        /// </summary>
        Error = 4,
        /// <summary>
        /// A debug message
        /// </summary>
        Debug = 8,
        /// <summary>
        /// All types of messages
        /// </summary>
        All = 15,
    }
}
