using Microsoft.Extensions.Logging;
using Rappen.XRM.RappSack;

namespace Rappen.XRM.RappSackDV
{
    public class RappSackDVTracerCore : RappSackTracerCore
    {
        private ILogger logger;

        public RappSackDVTracerCore(ILogger log) : base(TraceTiming.None)
        {
            logger = log;
        }

        protected override void TraceInternal(string message, string timestamp, int indent, TraceLevel level = TraceLevel.Information)
        {
            message = $"{timestamp}{new string(' ', indent * 2)}{message}";
            switch (level)
            {
                case TraceLevel.Critical:
                    logger.LogCritical(message);
                    break;

                case TraceLevel.Error:
                    logger.LogError(message);
                    break;

                case TraceLevel.Warning:
                    logger.LogWarning(message);
                    break;

                case TraceLevel.Debug:
                    logger.LogDebug(message);
                    break;

                case TraceLevel.Trace:
                    logger.LogTrace(message);
                    break;

                default:
                    logger.LogInformation(message);
                    break;
            }
        }
    }
}