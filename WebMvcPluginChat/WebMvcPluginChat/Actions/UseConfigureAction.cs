using System;
using ExtCore.Infrastructure.Actions;
using Microsoft.AspNetCore.Builder;

namespace WebMvcPluginChat.Actions
{
    public class UseConfigureAction : IConfigureAction
    {
        public int Priority => 1000;

        public void Execute(IApplicationBuilder app, IServiceProvider serviceProvider)
        {
        }
    }
}
