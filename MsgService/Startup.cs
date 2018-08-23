using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.HttpsPolicy;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Consul;
using Microsoft.AspNetCore.Http;
using AspectCore.Extensions.DependencyInjection;
using MsgService.Model;
using System.Reflection;
using RuPeng.HystrixCore;
using Swashbuckle.AspNetCore.Swagger;
using System.IO;

namespace MsgService
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public IServiceProvider ConfigureServices(IServiceCollection services)
        {
            services.AddMvc().SetCompatibilityVersion(CompatibilityVersion.Version_2_1);
            services.AddSwaggerGen(a =>
            {
                a.SwaggerDoc("vc", new Info()
                {
                    Contact = new Contact() { Name = "", Email = "", Url = "" },
                    Version = "v1.1.0",
                    Title = "Ray WebAPI",
                    Description = "框架集合",
                    TermsOfService = "None",
                });
                a.IncludeXmlComments("");
            });
            // services.AddSingleton<Person>();
            RegisterServices(this.GetType().Assembly, services);
            return services.BuildAspectCoreServiceProvider();
            //services.AddSingleton<Person>
        }

        //借助于 asp.net core 的依赖注入，简化代理类对象的注入，不用再自己调用 ProxyGeneratorBuilder 进行代理类对象的注入了
        public void RegisterServices(Assembly asm, IServiceCollection services)
        {
            foreach (Type type in asm.GetExportedTypes())
            {
                bool hasCustomInterceptorAttr = type.GetMethods().Any(m => m.GetCustomAttribute(typeof(HystrixCommandAttribute)) != null);
                if (hasCustomInterceptorAttr)
                {
                    services.AddSingleton(type);
                }
            }
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env, IApplicationLifetime applicationTime)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            else
            {
                app.UseHsts();
            }

            app.UseHttpsRedirection();
            app.UseMvc();
            #region Swagger
            app.UseSwagger();
            app.UseSwaggerUI();
            #endregion
            //在Asp.net使用httphandler和httpmodule做拦截等  在.net core中  middleware

            //app.Map("/values", mapApp =>
            //{
            //    mapApp.Use(async (context, next) =>
            //    {
            //        Console.WriteLine("1111");
            //        await context.Response.WriteAsync("Second Middleware in. \r\n");
            //        await next.Invoke();
            //        await context.Response.WriteAsync("Second Middleware out. \r\n");
            //    });
            //    mapApp.Run(async context =>
            //    {
            //        await context.Response.WriteAsync("Second. \r\n");
            //    });
            //});

            //注册服务 告诉consul我可以提供什么服务 告诉提供服务的ip等
            string ip = Configuration["ip"];
            string port = Configuration["port"];
            string serviceName = "MsgService";
            string serviceId = serviceName + Guid.NewGuid();
            //tcp/ip
            using (var consulClient = new ConsulClient(consulConfig))
            //using (var consulClient = new ConsulClient(c=> { c.Address = new Uri("http://127.0.0.1:8500");c.Datacenter = "dc1"; }))
            {
                AgentServiceRegistration asr = new AgentServiceRegistration();
                asr.Address = ip;
                asr.Port = Convert.ToInt32(port);
                asr.ID = serviceId;
                asr.Name = serviceName;
                asr.Check = new AgentServiceCheck()
                {
                    DeregisterCriticalServiceAfter = TimeSpan.FromSeconds(5),
                    HTTP = $"http://{ip}:{port}/api/Health",
                    Interval = TimeSpan.FromSeconds(10),
                    Timeout = TimeSpan.FromSeconds(5)
                };
                consulClient.Agent.ServiceRegister(asr).Wait();
            };
            applicationTime.ApplicationStopped.Register(() =>
            {
                using (var consulClient = new ConsulClient(consulConfig))
                {
                    Console.WriteLine("应用退出，开始从consul注销");
                    consulClient.Agent.ServiceDeregister(serviceId).Wait();
                }
            });
        }

        private void consulConfig(ConsulClientConfiguration c)
        {
            c.Address = new Uri("http://127.0.0.1:8500");
            c.Datacenter = "dc1";
        }
    }
}
