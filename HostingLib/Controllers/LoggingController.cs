using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Metadata;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using NLog;
using NLog.Config;
using NLog.Targets;

namespace HostingLib.Controllers
{
    public class LoggingController
    {
        private static readonly Logger logger = NLog.LogManager.Setup().LoadConfigurationFromFile("NLog.config").GetCurrentClassLogger();

        public static void LogInfo(string message)
        {
            logger.Info(message);
        }

        public static void LogDebug(string message)
        {
            logger.Debug(message);
        }

        public static void LogError(string message, Exception ex = null)
        {
            if (ex != null)
            {
                logger.Error(ex, message);
            }
            else
            {
                logger.Error(message);
            }
        }

        public static void LogWarning(string message)
        {
            logger.Warn(message);
        }

        public static void LogFatal(string message, Exception ex = null)
        {
            if (ex != null)
            {
                logger.Fatal(ex, message);
            }
            else
            {
                logger.Fatal(message);
            }
        }
    }
}
