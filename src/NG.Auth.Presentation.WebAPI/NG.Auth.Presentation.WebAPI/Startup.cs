using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using NG.Auth.Business.Impl.IoCModule;
using NG.Auth.Presentation.WebAPI.ModelBinder;
using NG.Common.Library.Extensions;
using NG.Common.Library.Filters;
using System.Reflection;
using System.Web.Mvc;

namespace NG.Auth.Presentation.WebAPI
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddMvc(options =>
            {
                RegisterBinderProvider registerBinderProvider = DependencyResolver.Current.GetService<RegisterBinderProvider>();
                options.ModelBinderProviders.Insert(0, registerBinderProvider);
                options.Filters.Add(typeof(ApiExceptionFilter));
            });

            services.AddControllers();

            services.Configure<IConfiguration>(Configuration);

            var xmlFile = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
            services.AddSwaggerDocumentation(Configuration.GetSection("Documentation"), xmlFile);

            services.AddJwtAuthentication(Configuration.GetSection("Secrets"));

            services.AddBusinessServices();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            app.UseLogScopeMiddleware();

            app.UseHttpsRedirection();

            app.UseRouting();

            app.UseSwaggerDocumentation(Configuration.GetSection("Documentation"));

            app.UseAuthentication();

            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
            });
        }
    }
}
