using System;
using System.IO;

namespace WebScriptHook.Framework
{
    public class Logger
    {
        public static string FileName
        {
            get;
            internal set;
        } = "WebScriptHook.log";

        public static LogType LogLevel
        {
            get;
            internal set;
        } = LogType.Info | LogType.Warning | LogType.Error;

        public static void Log(object message, LogType logType)
        {
            if ((LogLevel & logType) != LogType.None)
            {
                File.AppendAllText(FileName, "[" + DateTime.Now + "] [" + logType.ToString() + "]: " + message + Environment.NewLine);
            }
        }

        public static void Log(object message)
        {
            Log(message, LogType.Info);
        }
    }
}
