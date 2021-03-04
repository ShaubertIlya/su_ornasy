using Microsoft.Owin;
using Owin;

[assembly: OwinStartup(typeof(Devir.DMS.Web.Startup))]
namespace Devir.DMS.Web
{

    public class Startup
    {
        public void Configuration(IAppBuilder app)
        {
            app.MapSignalR();
        }
    }
}