using System;
using System.IO;
using System.Reflection;
using log4net;
using log4net.Config;

namespace CommercialFreeRadio.Impl
{
    public static class Logger
    {
        private static readonly ILog Log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);
        static Logger()
        {
            XmlConfigurator.ConfigureAndWatch(new FileInfo("log4netconfig.xml"));
        }
        public static void Debug(object m, params object[] args)
        {
            if (!IsDebugEnabled) return;
            Log.DebugFormat(m != null ? m.ToString() : string.Empty, args);
        }
        public static void Info(object m, params object[] args)
        {
            Log.InfoFormat(m != null ? m.ToString() : string.Empty, args);
        }
        public static void Error(Exception e)
        {
            Log.Error(e);
        }

        public static bool IsDebugEnabled { get; set; }
    }
}
