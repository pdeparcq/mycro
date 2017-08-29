using Nancy;

namespace Televic.Mycro.Web
{
    public class RootNancyModule : NancyModule
    {
        public RootNancyModule()
        {
            Get["/"] = x => View["index"];
        }
    }
}
