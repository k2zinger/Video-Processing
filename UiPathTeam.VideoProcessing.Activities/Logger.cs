using System.Diagnostics;

namespace UiPathTeam.VideoProcessing.Activities
{
    internal class Logger
    {
        private static Logger _instance;
        private TraceSource _traceSource;

        private Logger()
        {
            _traceSource = new TraceSource("Workflow");
        }

        public static Logger Instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = new Logger();
                }

                return _instance;
            }
        }

        public void Info(string message)
        {
            _traceSource.TraceEvent(TraceEventType.Information, 0, message);
        }

        public void Trace(string message)
        {
            _traceSource.TraceEvent(TraceEventType.Verbose, 0, message);
        }

        public void Error(string message)
        {
            _traceSource.TraceEvent(TraceEventType.Error, 0, message);
        }

        public void Warning(string message)
        {
            _traceSource.TraceEvent(TraceEventType.Warning, 0, message);
        }
    }
}
