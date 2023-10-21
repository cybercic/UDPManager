using log4net;
using System;
using System.Reflection;

namespace UDPManager
{
    public static class LogService
    {
        private static ILog logger = null;

        public enum LogType { Debug = 0, Info = 1, Error = 2, Warn = 3 }

        public static void GravaLog(Exception inner)
        {
            while (inner.InnerException != null)
                inner = inner.InnerException;

            GravaLog(string.Format("Mensagem:{0}", inner.Message), LogType.Error);
            GravaLog(string.Format("StackTrace:{0}", inner.StackTrace), LogType.Error);
        }

        public static void GravaLog(string message, LogType type)
        {
            log4net.Config.XmlConfigurator.Configure();

            logger = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

            switch (type)
            {
                case LogType.Debug:
                    logger.Debug(message);
                    break;
                case LogType.Info:
                    logger.Info(message);
                    break;
                case LogType.Error:
                    logger.Error(message);
                    break;
                case LogType.Warn:
                    logger.Warn(message);
                    break;
            }
        }
    }
}
