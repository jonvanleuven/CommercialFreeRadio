using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using log4net;
using log4net.Config;

namespace CommercialFreeRadio.Impl
{
    public static class Logger
    {
        private static readonly ILog Log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);
        private static List<Action<string>> infoListeners = new List<Action<string>>();

        static Logger()
        {
        }

        public static void Init(bool verbose)
        {
            XmlConfigurator.ConfigureAndWatch(new FileInfo(verbose ? "log4netconfigverbose.xml" : "log4netconfig.xml"));
        }

        public static void Debug(object m, params object[] args)
        {
            Log.DebugFormat(m != null ? m.ToString() : string.Empty, args);
        }
        public static void Info(object m, params object[] args)
        {
            Log.InfoFormat(m != null ? m.ToString() : string.Empty, args);
            infoListeners.ForEach(l => l(string.Format(m != null ? m.ToString() : string.Empty, args)));
        }
        public static void Error(Exception e)
        {
            Log.Error(e);
        }

        public static void AddInfoListerer(Action<string> infoListener)
        {
            infoListeners.Add(infoListener);
        }
    }
}
