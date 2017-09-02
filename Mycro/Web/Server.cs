using System;
using Autofac;
using Microsoft.Owin.Hosting;
using NLog;
using Quartz;

namespace Televic.Mycro.Web
{
    public class Server<T> where T : class, IStartup
    {
        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();
        private readonly string _hostUrl;
        private IContainer _container;
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
                var startup = Activator.CreateInstance(typeof(T)) as IStartup;
                if(startup == null) throw new ApplicationException("Failed to create startup instance!");
                _app = WebApp.Start(_hostUrl, builder => startup.Configure(builder));
                _container = startup.Container;
                _container.Resolve<IScheduler>().Start();
                Logger.Info("Server successfully started!");
                OnStarted(startup.Container);
            }
            catch (Exception e)
            {
                Logger.Error(e, "Starting server failed");
                throw;
            }
        }

        public virtual void OnStarted(IContainer container) { }

        public virtual void OnStopping(IContainer container) { }

        public void Stop()
        {
            Logger.Info("Stopping server");
            OnStopping(_container);
            _container.Resolve<IScheduler>().Shutdown();
            _app.Dispose();
        }
    }
}
