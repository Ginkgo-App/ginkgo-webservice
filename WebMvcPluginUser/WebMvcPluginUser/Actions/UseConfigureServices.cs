using System;
using APICore.Services;
using APICore.Services.Interfaces;
using ExtCore.Infrastructure.Actions;
using Microsoft.Extensions.DependencyInjection;

namespace WebMvcPluginUser.Actions
{
    public class UseConfigureServices : IConfigureServicesAction
    {
        public int Priority => 1000;

        public void Execute(IServiceCollection services, IServiceProvider serviceProvider)
        {
            services.AddSingleton<IUserService, UserService>();
        }
    }
}
