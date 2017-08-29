using System;
using Microsoft.Owin.Hosting;
using NLog;

namespace Televic.Mycro.Web
{
    public class Server<T>
    {
        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();
        private readonly string _hostUrl;
        private IDisposable _app;

        public Server(string hostUrl = "http://localhost:9000")
        {
            _hostUrl = hostUrl;
        }

        public void Start()
        {
            Logger.Info($"Starting server at {_hostUrl}");
            try
            {
                _app = WebApp.Start<T>(_hostUrl);
                Logger.Info("Server successfully started!");
            }
            catch (Exception e)
            {
                Logger.Error(e, "Starting server failed");
            }
        }

        public void Stop()
        {
            _app?.Dispose();
        }
    }
}
