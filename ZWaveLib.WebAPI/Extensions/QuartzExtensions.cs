using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Quartz;
using Quartz.Impl;
using Quartz.Spi;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;

namespace ZWaveLib.WebAPI.Extensions
{
    public static class QuartzExtensions
    {
        public static void UseQuartz(this IApplicationBuilder app)
        {
            app.UseQuartz(null);
        }

        public static void UseQuartz(this IApplicationBuilder app, Action<IScheduler> createScheduleJobs)
        {
            IScheduler scheduler = app.ApplicationServices.GetService<IScheduler>();
            scheduler.JobFactory = app.ApplicationServices.GetService<IJobFactory>();

            createScheduleJobs?.Invoke(scheduler);

            scheduler.Start();
        }

        public static void AddQuartz(this IServiceCollection services)
        {
            AddQuartz(services, null);
        }

        public static async void AddQuartz(this IServiceCollection services, Action<NameValueCollection> configuration)
        {
            var props = new NameValueCollection();
            configuration?.Invoke(props);

            ISchedulerFactory factory = new StdSchedulerFactory(props);
            IScheduler scheduler = await factory.GetScheduler();

            services.AddSingleton(scheduler);
            services.AddSingleton<IJobFactory, JobFactory>();

            foreach (TypeInfo typeInfo in Assembly.GetEntryAssembly().DefinedTypes)
                if (typeInfo.ImplementedInterfaces.Contains(typeof(IJob)))
                    services.AddTransient(typeInfo);
        }

        public static void CreateScheduleJob<TJob>(this IScheduler scheduler, Func<TriggerBuilder, TriggerBuilder> action) where TJob : IJob
        {
            scheduler.CreateScheduleJobAsync<TJob>(typeof(TJob).Name, action).Wait();
        }

        public static async Task CreateScheduleJobAsync<TJob>(this IScheduler scheduler, Func<TriggerBuilder, TriggerBuilder> action) where TJob : IJob
        {
            await scheduler.CreateScheduleJobAsync<TJob>(typeof(TJob).Name, action);
        }

        public static void CreateScheduleJob<TJob>(this IScheduler scheduler, string jobName, Func<TriggerBuilder, TriggerBuilder> action) where TJob : IJob
        {
            scheduler.CreateScheduleJobAsync<TJob>(jobName, action).Wait();
        }

        public static async Task CreateScheduleJobAsync<TJob>(this IScheduler scheduler, string jobName, Func<TriggerBuilder, TriggerBuilder> action) where TJob : IJob
        {
            IJobDetail job = JobBuilder.Create<TJob>()
                .WithIdentity(jobName)
                .Build();

            TriggerBuilder triggerBuilder = TriggerBuilder.Create().WithIdentity(jobName);
            ITrigger trigger = action(triggerBuilder).Build();

            await scheduler.ScheduleJob(job, trigger);
        }

        public class JobFactory : IJobFactory
        {
            private readonly IServiceScopeFactory serviceScopeFactory;

            private ConcurrentDictionary<int, IServiceScope> scopes;

            public JobFactory(IServiceScopeFactory serviceScopeFactory)
            {
                this.serviceScopeFactory = serviceScopeFactory;
                this.scopes = new ConcurrentDictionary<int, IServiceScope>();
            }

            public IJob NewJob(TriggerFiredBundle bundle, IScheduler scheduler)
            {
                IServiceScope serviceScope = serviceScopeFactory.CreateScope();

                IJob job = serviceScope.ServiceProvider.GetService(bundle.JobDetail.JobType) as IJob;
                scopes.TryAdd(job.GetHashCode(), serviceScope);

                return job;
            }

            public void ReturnJob(IJob job)
            {
                int hashcode = job.GetHashCode();

                try
                {
                    (job as IDisposable)?.Dispose();
                }
                catch { /* ignore */ }

                try
                {
                    scopes.TryRemove(hashcode, out IServiceScope scope);
                    scope.Dispose();
                }
                catch { /* ignore */ }
            }
        }
    }
}
