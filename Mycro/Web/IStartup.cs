using Autofac;
using Owin;

namespace Televic.Mycro.Web
{
    public interface IStartup
    {
        IContainer Container { get; }
        void Configure(IAppBuilder appBuilder);
    }
}
