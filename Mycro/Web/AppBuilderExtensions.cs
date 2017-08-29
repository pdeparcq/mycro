using System;
using System.Threading;
using Microsoft.Owin.BuilderProperties;
using Owin;

namespace Televic.Mycro.Web
{
    public static class AppBuilderExtensions
    {
        public static void OnDisposing(this IAppBuilder app, Action cleanup)
        {
            var properties = new AppProperties(app.Properties);
            var token = properties.OnAppDisposing;
            if (token != CancellationToken.None)
            {
                token.Register(cleanup);
            }
        }
    }
}
