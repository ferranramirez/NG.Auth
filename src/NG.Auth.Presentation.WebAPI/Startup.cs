using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using NG.Auth.Business.Contract.InternalServices;
using NG.Auth.Business.Impl.InternalServices;
using NG.Auth.Business.Impl.IoCModule;
using NG.Common.Library.Extensions;
using NG.Common.Library.Filters;
using System.Reflection;

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
                options.Filters.Add(typeof(ApiExceptionFilter));
            });

            services.AddHealthCheckMiddleware(Configuration);

            services.AddCors();
            
            services.AddControllers();

            var xmlFile = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
            services.AddSwaggerDocumentation(Configuration.GetSection("Documentation"), xmlFile);

            services.AddJwtAuthentication(Configuration.GetSection("Token"));

            services.AddDistributedRedisCache(option =>
            {
                option.Configuration = Configuration.GetConnectionString("Redis");
            });

            services.AddBusinessServices(Configuration);
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment()) app.UseDeveloperExceptionPage();

            app.UseErrorDisplayMiddleware();

            app.UseHealthCheckMiddleware();

            app.UseHttpsRedirection();

            app.UseCors();

            app.UseRouting();

            app.UseSwaggerDocumentation(Configuration.GetSection("Documentation"));

            app.UseAuthentication();

            app.UseAuthorization();

            app.UseLogScopeMiddleware();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
            });
        }
    }
}
