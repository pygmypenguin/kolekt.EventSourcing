using System;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using kolekt.EventSourcing.Extensions;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using kolekt.Data;
using Microsoft.EntityFrameworkCore;
using DemoApp.Messages;
using kolekt.EventSourcing.Providers;

namespace DemoApp
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
            services.AddControllers();

            services.Configure<HealthCheckPublisherOptions>(options =>
            {
                options.Delay = TimeSpan.FromSeconds(2);
                options.Predicate = (check) => check.Tags.Contains("ready");
            });

            var rabbitMqConfig = new RabbitMqHostOptions();
            Configuration.GetSection("rabbitMq").Bind(rabbitMqConfig);
            services.ConfigureMessageServicesWithRabbitMQ(rabbitMqConfig, includeHealthChecks: true);
            services.AddCommandValidator<DemoCommandValidator>();

            var dbConnection = Configuration.GetConnectionString("EventStoreConnectionString");
            services.AddDbContext<EventStoreDataContext>(options => options.UseSqlServer(dbConnection));
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            app.UseHttpsRedirection();

            app.UseRouting();

            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();

                endpoints.MapHealthChecks("/health/ready", new HealthCheckOptions()
                {
                    Predicate = (check) => check.Tags.Contains("ready")
                });

                endpoints.MapHealthChecks("/health/live", new HealthCheckOptions()
                {
                    Predicate = (_) => false
                });
            });
        }
    }
}
