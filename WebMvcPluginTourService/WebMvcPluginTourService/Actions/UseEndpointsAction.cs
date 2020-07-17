using System;
using ExtCore.Mvc.Infrastructure.Actions;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Routing;

namespace WebMvcPluginTourService.Actions
{
    class UseEndpointsAction : IUseEndpointsAction
    {
        public int Priority => 1000;

        public void Execute(IEndpointRouteBuilder endpointRouteBuilder, IServiceProvider serviceProvider)
        {
            endpointRouteBuilder.MapControllerRoute(
                name: "Default",
                pattern: "{controller=Home}/{action=Index}/{Id?}");
        }
    }
}
