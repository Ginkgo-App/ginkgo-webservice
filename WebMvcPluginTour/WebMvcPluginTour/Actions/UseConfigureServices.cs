﻿using ExtCore.Infrastructure.Actions;
using Microsoft.Extensions.DependencyInjection;
using System;
using APICore.Services;
using APICore.Services.Interfaces;

namespace WebMvcPluginTour.Actions
{
    public class UseConfigureServices : IConfigureServicesAction
    {
        public int Priority => 1000;

        public void Execute(IServiceCollection services, IServiceProvider serviceProvider)
        {
            services.AddSingleton<IUserService, UserService>();
            services.AddSingleton<ITourInfoService, TourInfoService>();
        }
    }
}
