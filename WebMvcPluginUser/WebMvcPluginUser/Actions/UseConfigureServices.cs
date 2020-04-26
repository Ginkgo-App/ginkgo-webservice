using ExtCore.Infrastructure.Actions;
using Microsoft.Extensions.DependencyInjection;
using System;
using WebMvcPluginUser.Services;

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
