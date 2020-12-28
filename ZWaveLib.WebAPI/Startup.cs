using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.OpenApi.Models;
using Quartz;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ZWaveLib.WebAPI.Extensions;
using ZWaveLib.WebAPI.ScheduledJobs;
using ZWaveLib.WebAPI.Services;

namespace ZWaveLib.WebAPI
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        public void ConfigureServices(IServiceCollection services)
        {
            services.AddControllers();
            services.AddSwaggerGen(c =>
            {
                c.SwaggerDoc("v1", new OpenApiInfo { Title = "ZWaveLib.WebAPI", Version = "v1" });
            });

            services.AddSingleton(x => new ZWaveController(Configuration["ZWave:Port"]));

            services.AddSingleton<IZWaveEventService, ZWaveEventService>();

            services.AddQuartz();
        }

        public void Configure(IApplicationBuilder app, IWebHostEnvironment env, IZWaveEventService eventService)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
                app.UseSwagger();
                app.UseSwaggerUI(c => c.SwaggerEndpoint("/swagger/v1/swagger.json", "ZWaveLib.WebAPI v1"));
            }

            eventService.Initialize();

            app.UseRouting();
            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
            });

            if (Configuration.GetValue<string>("ZWave:HealNetworkCron") != null)
            {
                app.UseQuartz(q =>
                {
                    q.CreateScheduleJob<HealNetworkScheduledJob>(s => s.WithCronSchedule(Configuration.GetValue<string>("ZWave:HealNetworkCron")));
                });
            }
        }
    }
}
