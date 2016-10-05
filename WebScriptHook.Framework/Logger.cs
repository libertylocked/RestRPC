using System;
using System.IO;
using System.Diagnostics;

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
                string formatedMessage = "[" + DateTime.Now + "] [" + logType.ToString() + "]: " + message;
                File.AppendAllText(FileName, formatedMessage + Environment.NewLine);
#if DEBUG
                Debug.WriteLine(formatedMessage);
#endif
            }
        }

        public static void Log(object message)
        {
            Log(message, LogType.Info);
        }
    }
}
