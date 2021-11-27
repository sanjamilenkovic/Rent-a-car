using Microsoft.Owin;
using Owin;

[assembly: OwinStartupAttribute(typeof(RentACarWebApp.Startup))]
namespace RentACarWebApp
{
    public partial class Startup
    {
        public void Configuration(IAppBuilder app)
        {
            ConfigureAuth(app);
        }
    }
}
