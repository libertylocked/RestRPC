using System;
using System.IO;
using System.Diagnostics;

namespace WebScriptHook.Framework
{
    /// <summary>
    /// Logger writes logs to a TextWriter
    /// </summary>
    public class Logger
    {
        /// <summary>
        /// Gets the TextWriter logger is writing to
        /// </summary>
        public static TextWriter Writer
        {
            get;
            internal set;
        }

        /// <summary>
        /// Gets the log level
        /// </summary>
        public static LogType LogLevel
        {
            get;
            internal set;
        } = LogType.Info | LogType.Warning | LogType.Error;

        /// <summary>
        /// Writes a log message using a specified log writer. Default writer will not be used for this message.
        /// </summary>
        /// <param name="message"></param>
        /// <param name="logType"></param>
        /// <param name="textWriter"></param>
        public static void Log(object message, LogType logType, TextWriter textWriter)
        {
            if ((LogLevel & logType) != LogType.None)
            {
                string formatedMessage = "[" + DateTime.Now + "] [" + logType.ToString() + "]: " + message;
                textWriter.WriteLine(formatedMessage);
            }
        }

        /// <summary>
        /// Writes a log message using the default log writer
        /// </summary>
        /// <param name="message"></param>
        /// <param name="logType"></param>
        public static void Log(object message, LogType logType)
        {
            if (Writer != null) Log(message, logType, Writer);
        }

        ///// <summary>
        ///// Writes an Info log message using the default log writer
        ///// </summary>
        ///// <param name="message"></param>
        //public static void Log(object message)
        //{
        //    Log(message, LogType.Info);
        //}
    }
}
