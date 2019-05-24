using Microsoft.Owin;
using Owin;

[assembly: OwinStartupAttribute(typeof(Sarang.Startup))]
namespace Sarang
{
    public partial class Startup
    {
        public void Configuration(IAppBuilder app)
        {
            ConfigureAuth(app);
        }
    }
}
