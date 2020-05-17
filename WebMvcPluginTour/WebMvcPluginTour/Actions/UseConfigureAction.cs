using ExtCore.Infrastructure.Actions;
using Microsoft.AspNetCore.Builder;
using System;

namespace WebMvcPluginTour.Actions
{
    public class UseConfigureAction : IConfigureAction
    {
        public int Priority => 1000;

        public void Execute(IApplicationBuilder app, IServiceProvider serviceProvider)
        {
        }
    }
}
