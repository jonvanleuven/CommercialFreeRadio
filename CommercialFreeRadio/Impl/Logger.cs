using System;

namespace CommercialFreeRadio.Impl
{
    public static class Logger
    {
        static Logger()
        {
            DateTimePattern = "dd-MM-yyyy HH:mm:ss";
        }
        public static void Debug(object m, params object[] args)
        {
            if (!IsDebugEnabled) return;
            Console.WriteLine(string.Format("{0} DEBUG {1}", Now(), string.Format(m != null ? m.ToString() : string.Empty, args)));
        }
        public static void Info(object m, params object[] args)
        {
            Console.WriteLine(string.Format("{0} INFO  {1}", Now(), string.Format(m != null ? m.ToString() : string.Empty, args)));
        }
        public static void Error(Exception e)
        {
            Console.WriteLine(string.Format("{0} ERROR {1}\n{2}", Now(), e.Message, e.StackTrace));
        }

        private static string Now()
        {
            return string.Format(DateTime.Now.ToString(DateTimePattern));
        }

        public static bool IsDebugEnabled { get; set; }
        public static string DateTimePattern { get; set; }
    }
}
