using ExtCore.WebApplication.Extensions;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.IdentityModel.Tokens;
using System.IO;
using System.Reflection;
using System.Text;
using APICore.DBContext;
using Toycloud.AspNetCore.Mvc.ModelBinding;
using APICore.Middlewares;
using APICore.Services;
using APICore.Services.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace APICore
{
    public class Startup
    {
        private readonly IWebHostEnvironment _webHostEnvironment;
        private readonly string _pluginsPath;

        public Startup(IConfiguration configuration, IWebHostEnvironment webHostEnvironment)
        {
            _webHostEnvironment = webHostEnvironment;
            Configuration = configuration;

            var contentRootPath = webHostEnvironment.ContentRootPath;
            if (webHostEnvironment.IsDevelopment())
            {
                // development
                contentRootPath = Path.GetDirectoryName(Assembly.GetEntryAssembly().Location);
            }

            _pluginsPath = Path.Combine(contentRootPath, "Plugins");
        }

        private IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddSingleton<IUserService, UserService>();
            services.AddSingleton<IFriendService, FriendService>();
            services.AddSingleton<ITourInfoService, TourInfoService>();
            services.AddSingleton<ITourService, TourService>();
            services.AddSingleton<IPlaceService, PlaceService>();
            
            services.AddExtCore(this._pluginsPath);

            services.AddControllers();

            services.AddCors();

            services.AddMvc(option =>
            {
                option.EnableEndpointRouting = false;
                option.ModelBinderProviders.InsertBodyOrDefaultBinding();
            }).AddNewtonsoftJson().SetCompatibilityVersion(CompatibilityVersion.Version_3_0);

            // configure strongly typed settings objects
            var appSettingsSection = Configuration.GetSection("AppSettings");
            services.Configure<AppSettings>(appSettingsSection);
            var appSettings = appSettingsSection.Get<AppSettings>();

            // configure jwt authentication
            var key = Encoding.ASCII.GetBytes(appSettings.Secret);
            services.AddAuthentication(x =>
            {
                x.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                x.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
            })
            .AddJwtBearer(x =>
            {
                x.RequireHttpsMetadata = false;
                x.SaveToken = true;
                x.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuerSigningKey = true,
                    IssuerSigningKey = new SymmetricSecurityKey(key),
                    ValidateIssuer = false,
                    ValidateAudience = false
                };
            });

            // Add global variables
            Vars.ConnectionString = appSettings.ConnectionString; 
            Vars.PasswordSalt = appSettings.PasswordSalt;

            services.AddDbContext<PostgreSQLContext>(options =>
            {
                options.UseNpgsql(Vars.ConnectionString, options => options.EnableRetryOnFailure());
            }, ServiceLifetime.Transient);
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            //app.UseExtCore();
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            app.UseAuthentication();
            app.UseCorsMiddleware();

            //app.UseHttpsRedirection();
            app.UseRouting();

            // global cors policy
            app.UseCors(x => x
                .AllowAnyOrigin()
                .AllowAnyMethod()
                .AllowAnyHeader());

            app.UseMvc();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllerRoute(
                    name: "default",
                    pattern: "{controller=Home}/{action=Index}/{Id?}");
            });
        }
    }
}
