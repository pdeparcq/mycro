using System.Collections.Generic;
using Nancy;
using Nancy.Bootstrapper;
using Nancy.Conventions;
using Nancy.TinyIoc;

namespace Televic.Mycro.Web
{
    public class CustomNancyBootstrapper : DefaultNancyBootstrapper
    {
        protected override void ApplicationStartup(TinyIoCContainer container, IPipelines pipelines)
        {
            base.ApplicationStartup(container, pipelines);

            Conventions.ViewLocationConventions.Insert(0, (viewName, model, context) => string.Concat("web/", viewName));
        }

        protected override void ConfigureConventions(NancyConventions nancyConventions)
        {
            base.ConfigureConventions(nancyConventions);

            nancyConventions.StaticContentsConventions.Add(
                StaticContentConventionBuilder.AddDirectory("/", @"web")
            );
        }

        protected override IEnumerable<ModuleRegistration> Modules => new[]
        {
            new ModuleRegistration(typeof(RootNancyModule)),
        };
    }
}
