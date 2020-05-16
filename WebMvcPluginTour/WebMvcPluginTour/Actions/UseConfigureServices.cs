using ExtCore.Infrastructure.Actions;
using Microsoft.Extensions.DependencyInjection;
using System;
using WebMvcPluginTour.Services;

namespace WebMvcPluginTour.Actions
{
    public class UseConfigureServices : IConfigureServicesAction
    {
        public int Priority => 1000;

        public void Execute(IServiceCollection services, IServiceProvider serviceProvider)
        {
            services.AddSingleton<ITourInfoService, TourInfoService>();
        }
    }
}
